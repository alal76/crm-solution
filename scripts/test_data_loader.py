#!/usr/bin/env python3

import argparse
import json
import os
import re
import sys
import urllib.error
import urllib.request
from datetime import datetime
from typing import Any, Dict, List, Optional, Tuple


def slugify(value: str) -> str:
    value = value.strip().lower()
    value = re.sub(r"[^a-z0-9]+", "-", value)
    return value.strip("-")


def email_from_name(name: str, website: Optional[str] = None) -> str:
    if website:
        match = re.search(r"https?://([^/]+)", website)
        if match:
            return f"info@{match.group(1)}"
    return f"info@{slugify(name)}.example.com"


def split_name(full_name: str) -> Tuple[str, str]:
    parts = full_name.strip().split()
    if len(parts) == 1:
        return parts[0], ""
    return parts[0], " ".join(parts[1:])


class RunLogger:
    def __init__(self, log_dir: str) -> None:
        os.makedirs(log_dir, exist_ok=True)
        timestamp = datetime.utcnow().strftime("%Y%m%d_%H%M%S")
        self.text_path = os.path.join(log_dir, f"test_data_load_{timestamp}.log")
        self.jsonl_path = os.path.join(log_dir, f"test_data_load_{timestamp}.jsonl")
        self.text_file = open(self.text_path, "w", encoding="utf-8")
        self.jsonl_file = open(self.jsonl_path, "w", encoding="utf-8")
        self.counts = {"success": 0, "failed": 0, "skipped": 0}

    def close(self) -> None:
        self.text_file.close()
        self.jsonl_file.close()

    def log(self, entry: Dict[str, Any]) -> None:
        entry.setdefault("timestamp", datetime.utcnow().isoformat() + "Z")
        line = json.dumps(entry, ensure_ascii=True)
        self.jsonl_file.write(line + "\n")
        self.jsonl_file.flush()
        summary = entry.get("summary")
        if summary:
            self.text_file.write(summary + "\n")
            self.text_file.flush()
        status = entry.get("status")
        if status in self.counts:
            self.counts[status] += 1

    def log_skip(self, reason: str, **kwargs: Any) -> None:
        entry = {
            "status": "skipped",
            "reason": reason,
        }
        entry.update(kwargs)
        entry["summary"] = f"SKIP: {reason} ({kwargs.get('file', 'n/a')})"
        self.log(entry)

    def log_result(
        self,
        status: str,
        method: str,
        endpoint: str,
        http_status: Optional[int],
        file: Optional[str],
        index: Optional[int],
        request_summary: Dict[str, Any],
        response_body: Optional[str] = None,
        error: Optional[str] = None,
    ) -> None:
        entry = {
            "status": status,
            "method": method,
            "endpoint": endpoint,
            "http_status": http_status,
            "file": file,
            "index": index,
            "request": request_summary,
            "response_body": response_body,
            "error": error,
        }
        line_info = f"{file}[{index}]" if file is not None and index is not None else "n/a"
        entry["summary"] = f"{status.upper()}: {method} {endpoint} ({line_info}) -> {http_status}"
        self.log(entry)


class ApiClient:
    def __init__(self, base_url: str, token: str, logger: RunLogger) -> None:
        self.base_url = base_url.rstrip("/")
        self.token = token
        self.logger = logger

    def request_json(
        self,
        method: str,
        path: str,
        payload: Optional[Dict[str, Any]],
        file: Optional[str] = None,
        index: Optional[int] = None,
        request_summary: Optional[Dict[str, Any]] = None,
    ) -> Tuple[Optional[int], Optional[Dict[str, Any]], Optional[str]]:
        url = f"{self.base_url}{path}"
        data = None
        if payload is not None:
            data = json.dumps(payload, ensure_ascii=True).encode("utf-8")
        request = urllib.request.Request(url, data=data, method=method)
        request.add_header("Authorization", f"Bearer {self.token}")
        request.add_header("Content-Type", "application/json")
        response_body = None
        try:
            with urllib.request.urlopen(request) as response:
                response_body = response.read().decode("utf-8")
                parsed = None
                if response_body:
                    try:
                        parsed = json.loads(response_body)
                    except json.JSONDecodeError:
                        parsed = None
                self.logger.log_result(
                    "success",
                    method,
                    path,
                    response.getcode(),
                    file,
                    index,
                    request_summary or {},
                    response_body=response_body,
                )
                return response.getcode(), parsed, response_body
        except urllib.error.HTTPError as ex:
            response_body = ex.read().decode("utf-8") if ex.fp else None
            self.logger.log_result(
                "failed",
                method,
                path,
                ex.code,
                file,
                index,
                request_summary or {},
                response_body=response_body,
                error=str(ex),
            )
            return ex.code, None, response_body
        except Exception as ex:
            self.logger.log_result(
                "failed",
                method,
                path,
                None,
                file,
                index,
                request_summary or {},
                error=str(ex),
            )
            return None, None, None


def load_json(path: str) -> Any:
    with open(path, "r", encoding="utf-8") as handle:
        return json.load(handle)


def main() -> int:
    parser = argparse.ArgumentParser(description="Load CRM test data via API with extensive logging")
    parser.add_argument("--base-url", default="http://localhost:5000", help="CRM API base URL")
    parser.add_argument("--data-dir", default="e2e-tests/test-data", help="Seed data directory")
    parser.add_argument("--username", default="admin@crm.local", help="Admin email")
    parser.add_argument("--password", default="Admin@123", help="Admin password")
    parser.add_argument("--log-dir", default="logs/test-data", help="Log output directory")
    args = parser.parse_args()

    logger = RunLogger(args.log_dir)

    try:
        auth_payload = {"email": args.username, "password": args.password}
        auth_request = urllib.request.Request(
            f"{args.base_url.rstrip('/')}/api/auth/login",
            data=json.dumps(auth_payload, ensure_ascii=True).encode("utf-8"),
            method="POST",
        )
        auth_request.add_header("Content-Type", "application/json")
        with urllib.request.urlopen(auth_request) as response:
            body = response.read().decode("utf-8")
            auth_data = json.loads(body)
        token = auth_data.get("accessToken") or auth_data.get("AccessToken") or auth_data.get("token")
        if not token:
            logger.log_result("failed", "POST", "/api/auth/login", 200, None, None, auth_payload, body, "Missing token")
            return 1
        logger.log_result("success", "POST", "/api/auth/login", 200, None, None, {"email": args.username}, body)
    except Exception as ex:
        logger.log_result("failed", "POST", "/api/auth/login", None, None, None, {"email": args.username}, error=str(ex))
        return 1

    client = ApiClient(args.base_url, token, logger)
    data_dir = os.path.abspath(args.data_dir)

    role_name_to_id: Dict[str, int] = {}
    user_id_map: Dict[int, int] = {}
    account_id_map: Dict[int, int] = {}
    account_name_to_id: Dict[str, int] = {}
    contact_id_map: Dict[int, int] = {}
    email_template_id_map: Dict[int, int] = {}
    category_name_to_id: Dict[str, int] = {}

    def load_roles() -> None:
        path = os.path.join(data_dir, "system_roles_seed.json")
        if not os.path.exists(path):
            return
        items = load_json(path)
        for index, item in enumerate(items):
            payload = {
                "Name": item.get("name", ""),
                "Description": item.get("description", ""),
                "HierarchyLevel": 0,
            }
            _, resp, _ = client.request_json("POST", "/api/roles", payload, path, index, payload)
            if isinstance(resp, dict) and "id" in resp:
                role_name_to_id[item.get("name", "")] = resp["id"]

    def load_permissions() -> None:
        path = os.path.join(data_dir, "system_permissions_seed.json")
        if not os.path.exists(path):
            return
        items = load_json(path)
        for index, item in enumerate(items):
            key = item.get("key", "")
            module = key.split(".")[0] if "." in key else "General"
            payload = {
                "Name": key,
                "DisplayName": key,
                "Module": module,
                "Category": "General",
                "Description": item.get("description", ""),
            }
            client.request_json("POST", "/api/permissions", payload, path, index, payload)

    def load_users() -> None:
        path = os.path.join(data_dir, "bulk_crm_seed.json")
        if not os.path.exists(path):
            return
        data = load_json(path)
        for index, item in enumerate(data.get("users", [])):
            role_name = item.get("role")
            payload = {
                "Email": item.get("email"),
                "FirstName": item.get("firstName"),
                "LastName": item.get("lastName"),
                "RoleId": role_name_to_id.get(role_name, 2),
                "Password": "Admin@123",
            }
            _, resp, _ = client.request_json("POST", "/api/users", payload, path, index, payload)
            if isinstance(resp, dict) and "id" in resp:
                user_id_map[item.get("id", 0)] = resp["id"]

    def load_user_groups() -> None:
        path = os.path.join(data_dir, "system_user_groups_seed.json")
        if not os.path.exists(path):
            return
        items = load_json(path)
        group_id_map: Dict[int, int] = {}
        for index, item in enumerate(items):
            payload = {
                "Name": item.get("name", ""),
                "Description": "",
                "IsActive": True,
            }
            _, resp, _ = client.request_json("POST", "/api/usergroups", payload, path, index, payload)
            if isinstance(resp, dict) and "id" in resp:
                group_id_map[item.get("id", 0)] = resp["id"]

        for item in items:
            group_id = group_id_map.get(item.get("id", 0))
            if not group_id:
                continue
            for user_id in item.get("memberUserIds", []):
                actual_user_id = user_id_map.get(user_id)
                if not actual_user_id:
                    logger.log_skip("User not found for group membership", file=path, groupId=group_id, userId=user_id)
                    continue
                client.request_json(
                    "POST",
                    f"/api/AdminSettings/groups/{group_id}/members/{actual_user_id}",
                    None,
                    path,
                    None,
                    {"groupId": group_id, "userId": actual_user_id},
                )

    def load_accounts() -> None:
        account_category_org = 1
        account_type_enterprise = 3
        account_priority_medium = 1
        bulk_path = os.path.join(data_dir, "bulk_crm_seed.json")
        if os.path.exists(bulk_path):
            data = load_json(bulk_path)
            for index, item in enumerate(data.get("accounts", [])):
                phone = item.get("phone") or "+1-555-0000"
                email = email_from_name(item.get("name", ""), item.get("website"))
                payload = {
                    "Category": account_category_org,
                    "Company": item.get("name"),
                    "Email": email,
                    "Phone": phone,
                    "Website": item.get("website"),
                    "Address": item.get("address"),
                    "Industry": item.get("industry"),
                    "AccountType": account_type_enterprise,
                    "Priority": account_priority_medium,
                }
                _, resp, _ = client.request_json("POST", "/api/accounts", payload, bulk_path, index, payload)
                if isinstance(resp, dict) and "id" in resp:
                    account_id_map[item.get("id", 0)] = resp["id"]
                    account_name_to_id[item.get("name", "")] = resp["id"]

        companies_path = os.path.join(data_dir, "it_companies_seed.json")
        if os.path.exists(companies_path):
            data = load_json(companies_path)
            for index, item in enumerate(data.get("accounts", [])):
                address = item.get("address", {})
                address_line = ", ".join(
                    part for part in [address.get("street"), address.get("city"), address.get("state"), address.get("postalCode")]
                    if part
                )
                phone = item.get("phone") or "+1-555-0000"
                email = email_from_name(item.get("name", ""), item.get("website"))
                payload = {
                    "Category": account_category_org,
                    "Company": item.get("name"),
                    "Email": email,
                    "Phone": phone,
                    "Website": item.get("website"),
                    "Address": address_line,
                    "City": address.get("city"),
                    "State": address.get("state"),
                    "ZipCode": address.get("postalCode"),
                    "Country": address.get("country"),
                    "Industry": item.get("industry"),
                    "AccountType": account_type_enterprise,
                    "Priority": account_priority_medium,
                }
                _, resp, _ = client.request_json("POST", "/api/accounts", payload, companies_path, index, payload)
                if isinstance(resp, dict) and "id" in resp:
                    account_name_to_id[item.get("name", "")] = resp["id"]

    def load_contacts() -> None:
        bulk_path = os.path.join(data_dir, "bulk_crm_seed.json")
        if os.path.exists(bulk_path):
            data = load_json(bulk_path)
            for index, item in enumerate(data.get("contacts", [])):
                payload = {
                    "FirstName": item.get("firstName"),
                    "LastName": item.get("lastName"),
                    "EmailPrimary": item.get("email"),
                    "PhonePrimary": item.get("phone"),
                }
                _, resp, _ = client.request_json("POST", "/api/contacts", payload, bulk_path, index, payload)
                if isinstance(resp, dict) and "id" in resp:
                    contact_id_map[item.get("id", 0)] = resp["id"]

                account_id = account_id_map.get(item.get("accountId", 0))
                contact_id = contact_id_map.get(item.get("id", 0))
                if account_id and contact_id:
                    link_payload = {"ContactId": contact_id}
                    client.request_json(
                        "POST",
                        f"/api/accounts/{account_id}/contacts",
                        link_payload,
                        bulk_path,
                        index,
                        link_payload,
                    )

        companies_path = os.path.join(data_dir, "it_companies_seed.json")
        if os.path.exists(companies_path):
            data = load_json(companies_path)
            for company_index, item in enumerate(data.get("accounts", [])):
                account_id = account_name_to_id.get(item.get("name", ""))
                for exec_index, exec_item in enumerate(item.get("executives", [])):
                    first_name, last_name = split_name(exec_item.get("name", ""))
                    payload = {
                        "FirstName": first_name,
                        "LastName": last_name,
                        "EmailPrimary": exec_item.get("email"),
                        "PhonePrimary": exec_item.get("phone"),
                        "JobTitle": exec_item.get("title"),
                        "Company": item.get("name"),
                    }
                    _, resp, _ = client.request_json(
                        "POST",
                        "/api/contacts",
                        payload,
                        companies_path,
                        exec_index,
                        payload,
                    )
                    contact_id = resp.get("id") if isinstance(resp, dict) else None
                    if account_id and contact_id:
                        link_payload = {"ContactId": contact_id}
                        client.request_json(
                            "POST",
                            f"/api/accounts/{account_id}/contacts",
                            link_payload,
                            companies_path,
                            exec_index,
                            link_payload,
                        )

    def load_leads() -> None:
        lead_status_map = {
            "New": 0,
            "Contacted": 1,
            "Qualified": 3,
        }
        path = os.path.join(data_dir, "bulk_crm_seed.json")
        if not os.path.exists(path):
            return
        data = load_json(path)
        for index, item in enumerate(data.get("leads", [])):
            payload = {
                "FirstName": item.get("firstName"),
                "LastName": item.get("lastName"),
                "Email": item.get("email"),
                "Phone": item.get("phone"),
                "Company": item.get("company"),
                "Status": lead_status_map.get(item.get("status"), 0),
            }
            client.request_json("POST", "/api/leads", payload, path, index, payload)

    def load_products() -> None:
        bulk_path = os.path.join(data_dir, "bulk_crm_seed.json")
        if os.path.exists(bulk_path):
            data = load_json(bulk_path)
            for index, item in enumerate(data.get("products", [])):
                payload = {
                    "Name": item.get("name"),
                    "SKU": item.get("sku"),
                    "Price": item.get("price"),
                    "Category": item.get("category"),
                    "IsActive": True,
                }
                client.request_json("POST", "/api/products", payload, bulk_path, index, payload)

        products_path = os.path.join(data_dir, "products_seed.json")
        if os.path.exists(products_path):
            data = load_json(products_path)
            for index, item in enumerate(data):
                payload = {
                    "Name": item.get("Name"),
                    "SKU": item.get("SKU"),
                    "Price": item.get("Price"),
                    "Category": item.get("Category"),
                    "Description": item.get("Description"),
                    "IsActive": item.get("Status") == "Active",
                }
                client.request_json("POST", "/api/products", payload, products_path, index, payload)

    def load_opportunities() -> None:
        stage_map = {
            "Discovery": "Discovery",
            "Qualified": "Qualification",
            "Qualification": "Qualification",
            "Proposal": "Proposal",
            "Negotiation": "Negotiation",
            "ClosedWon": "ClosedWon",
            "ClosedLost": "ClosedLost",
        }
        path = os.path.join(data_dir, "bulk_crm_seed.json")
        if not os.path.exists(path):
            return
        data = load_json(path)
        for index, item in enumerate(data.get("opportunities", [])):
            payload = {
                "Name": item.get("name"),
                "AccountId": account_id_map.get(item.get("accountId", 0)),
                "Amount": item.get("amount"),
                "Stage": stage_map.get(item.get("stage"), "Discovery"),
                "ExpectedCloseDate": item.get("closeDate"),
                "Currency": "USD",
            }
            client.request_json("POST", "/api/opportunities", payload, path, index, payload)

    def load_quotes_orders_invoices() -> None:
        path = os.path.join(data_dir, "sales_quotes_seed.json")
        if os.path.exists(path):
            for index, item in enumerate(load_json(path)):
                payload = {
                    "AccountId": account_id_map.get(item.get("accountId", 0)),
                    "ContactId": contact_id_map.get(item.get("contactId", 0)),
                    "Status": item.get("status"),
                    "TotalAmount": item.get("totalAmount"),
                    "ValidUntil": item.get("validUntil"),
                    "QuoteDate": item.get("createdDate"),
                    "OpportunityId": item.get("opportunityId"),
                }
                client.request_json("POST", "/api/quotes", payload, path, index, payload)

        path = os.path.join(data_dir, "sales_quote_line_items_seed.json")
        if os.path.exists(path):
            for index, item in enumerate(load_json(path)):
                payload = {
                    "ProductId": item.get("productId"),
                    "Quantity": item.get("quantity"),
                    "UnitPrice": item.get("unitPrice"),
                    "LineTotal": item.get("lineTotal"),
                }
                client.request_json(
                    "POST",
                    f"/api/quotes/{item.get('quoteId')}/lineitems",
                    payload,
                    path,
                    index,
                    payload,
                )

        path = os.path.join(data_dir, "sales_orders_seed.json")
        if os.path.exists(path):
            for index, item in enumerate(load_json(path)):
                payload = {
                    "AccountId": account_id_map.get(item.get("accountId", 0)),
                    "QuoteId": item.get("quoteId"),
                    "Status": item.get("status"),
                    "OrderDate": item.get("orderDate"),
                    "TotalAmount": item.get("totalAmount"),
                }
                client.request_json("POST", "/api/orders", payload, path, index, payload)

        path = os.path.join(data_dir, "sales_order_line_items_seed.json")
        if os.path.exists(path):
            for index, item in enumerate(load_json(path)):
                payload = {
                    "ProductId": item.get("productId"),
                    "Quantity": item.get("quantity"),
                    "UnitPrice": item.get("unitPrice"),
                    "LineTotal": item.get("lineTotal"),
                }
                client.request_json(
                    "POST",
                    f"/api/orders/{item.get('orderId')}/line-items",
                    payload,
                    path,
                    index,
                    payload,
                )

        path = os.path.join(data_dir, "sales_invoices_seed.json")
        if os.path.exists(path):
            for index, item in enumerate(load_json(path)):
                payload = {
                    "OrderId": item.get("orderId"),
                    "AccountId": account_id_map.get(item.get("accountId", 0)),
                    "InvoiceNumber": item.get("invoiceNumber"),
                    "Status": item.get("status"),
                    "IssueDate": item.get("issueDate"),
                    "DueDate": item.get("dueDate"),
                    "TotalAmount": item.get("totalAmount"),
                }
                client.request_json("POST", "/api/invoices", payload, path, index, payload)

        path = os.path.join(data_dir, "sales_invoice_line_items_seed.json")
        if os.path.exists(path):
            for index, item in enumerate(load_json(path)):
                payload = {
                    "ProductId": item.get("productId"),
                    "Description": item.get("description"),
                    "Quantity": item.get("quantity"),
                    "UnitPrice": item.get("unitPrice"),
                    "LineTotal": item.get("lineTotal"),
                }
                client.request_json(
                    "POST",
                    f"/api/invoices/{item.get('invoiceId')}/line-items",
                    payload,
                    path,
                    index,
                    payload,
                )

        path = os.path.join(data_dir, "sales_payments_seed.json")
        if os.path.exists(path):
            for index, item in enumerate(load_json(path)):
                payload = {
                    "InvoiceId": item.get("invoiceId"),
                    "Amount": item.get("amount"),
                    "Method": item.get("method"),
                    "Status": item.get("status"),
                    "PaymentDate": item.get("paymentDate"),
                    "TransactionRef": item.get("transactionRef"),
                }
                client.request_json("POST", "/api/payments", payload, path, index, payload)

        path = os.path.join(data_dir, "sales_contracts_seed.json")
        if os.path.exists(path):
            for index, item in enumerate(load_json(path)):
                payload = {
                    "AccountId": account_id_map.get(item.get("accountId", 0)),
                    "ContractNumber": item.get("contractNumber"),
                    "Status": item.get("status"),
                    "StartDate": item.get("startDate"),
                    "EndDate": item.get("endDate"),
                    "Value": item.get("value"),
                    "RenewalTermMonths": item.get("renewalTermMonths"),
                }
                client.request_json("POST", "/api/contracts", payload, path, index, payload)

        path = os.path.join(data_dir, "sales_subscriptions_seed.json")
        if os.path.exists(path):
            for index, item in enumerate(load_json(path)):
                payload = {
                    "AccountId": account_id_map.get(item.get("accountId", 0)),
                    "ProductId": item.get("productId"),
                    "Status": item.get("status"),
                    "StartDate": item.get("startDate"),
                    "EndDate": item.get("endDate"),
                    "BillingCycle": item.get("billingCycle"),
                    "Amount": item.get("amount"),
                }
                client.request_json("POST", "/api/subscriptions", payload, path, index, payload)

        path = os.path.join(data_dir, "sales_commissions_seed.json")
        if os.path.exists(path):
            for index, item in enumerate(load_json(path)):
                payload = {
                    "UserId": user_id_map.get(item.get("userId", 0)),
                    "OrderId": item.get("orderId"),
                    "Rate": item.get("rate"),
                    "Amount": item.get("amount"),
                    "Status": item.get("status"),
                    "Period": item.get("period"),
                }
                client.request_json("POST", "/api/commissions", payload, path, index, payload)

    def load_marketing() -> None:
        template_path = os.path.join(data_dir, "marketing_email_templates_seed.json")
        if os.path.exists(template_path):
            for index, item in enumerate(load_json(template_path)):
                payload = {
                    "Name": item.get("name"),
                    "Subject": item.get("subject"),
                    "Category": item.get("type") or "General",
                    "IsActive": item.get("status") == "Active",
                }
                _, resp, _ = client.request_json("POST", "/api/emailtemplates", payload, template_path, index, payload)
                if isinstance(resp, dict) and "id" in resp:
                    email_template_id_map[item.get("id", 0)] = resp["id"]

        sequence_path = os.path.join(data_dir, "marketing_email_sequences_seed.json")
        if os.path.exists(sequence_path):
            for index, item in enumerate(load_json(sequence_path)):
                steps = []
                for step_index, template_id in enumerate(item.get("templateIds", []), start=1):
                    steps.append({
                        "StepOrder": step_index,
                        "StepType": "Email",
                        "TemplateId": email_template_id_map.get(template_id, template_id),
                        "DelayDays": 2,
                    })
                payload = {
                    "Name": item.get("name"),
                    "Status": item.get("status"),
                    "IsActive": item.get("status") == "Active",
                    "Steps": steps,
                }
                client.request_json("POST", "/api/email-sequences", payload, sequence_path, index, payload)

        campaigns_path = os.path.join(data_dir, "marketing_campaigns_seed.json")
        if os.path.exists(campaigns_path):
            for index, item in enumerate(load_json(campaigns_path)):
                payload = {
                    "Name": item.get("name"),
                    "Type": item.get("type") or "Email",
                    "CampaignType": item.get("type") or "Email",
                    "Status": item.get("status"),
                    "StartDate": item.get("startDate"),
                    "EndDate": item.get("endDate"),
                    "Budget": item.get("budget"),
                    "OwnerId": user_id_map.get(item.get("ownerUserId", 0)),
                }
                client.request_json("POST", "/api/campaigns", payload, campaigns_path, index, payload)

    def load_service_requests() -> None:
        categories_path = os.path.join(data_dir, "service_request_categories_seed.json")
        if os.path.exists(categories_path):
            for index, item in enumerate(load_json(categories_path)):
                payload = {
                    "Name": item.get("Name"),
                    "Description": item.get("Description"),
                    "DisplayOrder": item.get("DisplayOrder"),
                    "IsActive": item.get("IsActive", True),
                    "IconName": item.get("IconName"),
                    "ColorCode": item.get("ColorCode"),
                    "DefaultResponseTimeHours": item.get("DefaultResponseTimeHours"),
                    "DefaultResolutionTimeHours": item.get("DefaultResolutionTimeHours"),
                }
                _, resp, _ = client.request_json(
                    "POST",
                    "/api/service-request-settings/categories",
                    payload,
                    categories_path,
                    index,
                    payload,
                )
                if isinstance(resp, dict) and "id" in resp:
                    category_name_to_id[item.get("Name", "")] = resp["id"]

        types_path = os.path.join(data_dir, "service_request_types_seed.json")
        if os.path.exists(types_path):
            for index, item in enumerate(load_json(types_path)):
                payload = {
                    "Name": item.get("Name"),
                    "RequestType": item.get("RequestType"),
                    "DetailedDescription": item.get("DetailedDescription"),
                    "WorkflowName": item.get("WorkflowName"),
                    "PossibleResolutions": ";".join(item.get("PossibleResolutions", [])),
                    "FinalCustomerResolutions": ";".join(item.get("FinalCustomerResolutions", [])),
                    "CategoryId": item.get("CategoryId"),
                    "SubcategoryId": item.get("SubcategoryId"),
                    "DisplayOrder": item.get("DisplayOrder"),
                    "IsActive": item.get("IsActive", True),
                    "DefaultPriority": item.get("DefaultPriority"),
                    "ResponseTimeHours": item.get("ResponseTimeHours"),
                    "ResolutionTimeHours": item.get("ResolutionTimeHours"),
                    "Tags": ",".join(item.get("Tags", [])),
                }
                client.request_json(
                    "POST",
                    "/api/service-request-settings/types",
                    payload,
                    types_path,
                    index,
                    payload,
                )

        requests_path = os.path.join(data_dir, "bulk_crm_seed.json")
        if os.path.exists(requests_path):
            data = load_json(requests_path)
            priority_map = {"Low": 0, "Medium": 1, "High": 2, "Critical": 3, "Urgent": 4}
            for index, item in enumerate(data.get("serviceRequests", [])):
                payload = {
                    "Subject": item.get("title"),
                    "Description": item.get("title"),
                    "Priority": priority_map.get(item.get("priority"), 1),
                    "CategoryId": category_name_to_id.get(item.get("category")),
                    "AccountId": account_id_map.get(item.get("accountId", 0)),
                    "ContactId": contact_id_map.get(item.get("contactId", 0)),
                }
                client.request_json("POST", "/api/servicerequests", payload, requests_path, index, payload)

    def load_itsm() -> None:
        cmdb_path = os.path.join(data_dir, "itsm_cmdb_items_seed.json")
        if os.path.exists(cmdb_path):
            for index, item in enumerate(load_json(cmdb_path)):
                payload = {
                    "CIName": item.get("name"),
                    "CIType": item.get("type"),
                    "CISubtype": item.get("category"),
                    "OperationalStatus": item.get("status"),
                    "OwnerId": account_id_map.get(item.get("ownerAccountId", 0)),
                }
                client.request_json("POST", "/api/itsm/cmdb", payload, cmdb_path, index, payload)

        incidents_path = os.path.join(data_dir, "itsm_incidents_seed.json")
        if os.path.exists(incidents_path):
            impact_map = {"High": "High", "Medium": "Medium", "Low": "Low"}
            for index, item in enumerate(load_json(incidents_path)):
                payload = {
                    "ShortDescription": item.get("title"),
                    "Description": item.get("title"),
                    "CallerId": contact_id_map.get(item.get("reportedByContactId", 0)) or 1,
                    "Impact": impact_map.get(item.get("priority"), "Medium"),
                    "Urgency": impact_map.get(item.get("priority"), "Medium"),
                }
                client.request_json("POST", "/api/itsm/incidents", payload, incidents_path, index, payload)

        problems_path = os.path.join(data_dir, "itsm_problems_seed.json")
        if os.path.exists(problems_path):
            priority_map = {"Critical": "Critical", "High": "High", "Medium": "Medium", "Low": "Low"}
            for index, item in enumerate(load_json(problems_path)):
                payload = {
                    "ShortDescription": item.get("title"),
                    "Description": item.get("title"),
                    "Priority": priority_map.get(item.get("priority"), "Medium"),
                    "IncidentIds": [item.get("relatedIncidentId")],
                }
                client.request_json("POST", "/api/itsm/problems", payload, problems_path, index, payload)

        changes_path = os.path.join(data_dir, "itsm_changes_seed.json")
        if os.path.exists(changes_path):
            for index, item in enumerate(load_json(changes_path)):
                payload = {
                    "ShortDescription": item.get("title"),
                    "Description": item.get("title"),
                    "Type": item.get("type"),
                    "Risk": item.get("risk"),
                    "Impact": item.get("risk"),
                    "PlannedStartDate": item.get("scheduledDate"),
                    "PlannedEndDate": item.get("scheduledDate"),
                }
                client.request_json("POST", "/api/itsm/changes", payload, changes_path, index, payload)

        sla_path = os.path.join(data_dir, "itsm_sla_policies_seed.json")
        if os.path.exists(sla_path):
            for index, item in enumerate(load_json(sla_path)):
                payload = {
                    "Name": item.get("PolicyName"),
                    "TargetType": 0,
                    "P1ResponseMinutes": item.get("ResponseTimeMinutes"),
                    "P1ResolutionMinutes": item.get("ResolutionTimeMinutes"),
                    "UseBusinessHours": item.get("BusinessHoursOnly", False),
                    "IsActive": item.get("IsActive", True),
                }
                client.request_json("POST", "/api/itsm/sla/policies", payload, sla_path, index, payload)

        knowledge_path = os.path.join(data_dir, "service_desk_knowledge_articles_seed.json")
        if os.path.exists(knowledge_path):
            for index, item in enumerate(load_json(knowledge_path)):
                payload = {
                    "Title": item.get("title"),
                    "ArticleBody": " ".join(item.get("tags", [])) or item.get("title"),
                    "ArticleType": "HowTo",
                    "ShortDescription": item.get("category"),
                    "IsInternal": False,
                }
                client.request_json("POST", "/api/itsm/knowledge", payload, knowledge_path, index, payload)

    def load_workflows() -> None:
        path = os.path.join(data_dir, "service_desk_workflow_definitions_seed.json")
        if not os.path.exists(path):
            return
        for index, item in enumerate(load_json(path)):
            name = item.get("name")
            payload = {
                "WorkflowKey": slugify(name),
                "Name": name,
                "Category": item.get("module"),
                "EntityType": "ServiceRequest",
                "Tags": item.get("steps", []),
            }
            client.request_json("POST", "/api/workflows", payload, path, index, payload)

    def load_feature_flags() -> None:
        path = os.path.join(data_dir, "system_feature_flags_seed.json")
        if not os.path.exists(path):
            return
        for index, item in enumerate(load_json(path)):
            payload = {
                "Name": item.get("name"),
                "Enabled": item.get("enabled", False),
                "RolloutPercentage": 100,
                "Reason": "Seed data load",
            }
            client.request_json("PUT", f"/api/feature-flags/{item.get('name')}", payload, path, index, payload)

    def load_system_settings() -> None:
        path = os.path.join(data_dir, "system_settings_seed.json")
        if not os.path.exists(path):
            return
        update_payload: Dict[str, Any] = {}
        for item in load_json(path):
            key = item.get("key")
            value = item.get("value")
            if key == "System.TimeZone":
                update_payload["DefaultTimezone"] = value
            elif key == "System.DateFormat":
                update_payload["DateFormat"] = value
            elif key == "Security.PasswordMinLength":
                update_payload["MinPasswordLength"] = int(value)
            elif key == "Security.MfaRequired":
                update_payload["RequireTwoFactor"] = str(value).lower() == "true"
            elif key == "Sales.DefaultCurrency":
                update_payload["DefaultCurrency"] = value
            else:
                logger.log_skip("No mapping for system setting", file=path, key=key)

        if update_payload:
            client.request_json("PUT", "/api/systemsettings", update_payload, path, None, update_payload)

    def load_unsupported() -> None:
        for name in [
            "analytics_events_seed.json",
            "ai_agent_usage_seed.json",
            "integration_export_jobs_seed.json",
            "integration_import_jobs_seed.json",
            "integration_webhooks_seed.json",
            "service_desk_escalation_rules_seed.json",
            "itsm_catalog_categories_seed.json",
            "itsm_change_types_seed.json",
            "itsm_ci_types_seed.json",
            "itsm_incident_categories_seed.json",
            "marketing_campaign_conversions_seed.json",
            "marketing_campaign_metrics_seed.json",
            "marketing_campaign_recipients_seed.json",
            "services_seed.json",
            "system_audit_logs_seed.json",
        ]:
            logger.log_skip("No supported API endpoint", file=os.path.join(data_dir, name))

    load_roles()
    load_permissions()
    load_users()
    load_accounts()
    load_contacts()
    load_user_groups()
    load_leads()
    load_products()
    load_opportunities()
    load_quotes_orders_invoices()
    load_marketing()
    load_service_requests()
    load_itsm()
    load_workflows()
    load_feature_flags()
    load_system_settings()
    load_unsupported()

    logger.log(
        {
            "status": "success",
            "summary": f"Run complete. Success={logger.counts['success']} Failed={logger.counts['failed']} Skipped={logger.counts['skipped']}",
        }
    )
    logger.close()
    return 0


if __name__ == "__main__":
    sys.exit(main())
