import {
  QuoteStatusEnum,
  QuoteStatus,
  quoteStatusFromApi,
  quoteStatusToApi,
  OrderStatus,
} from '../types/sales';

describe('QuoteStatus mappings', () => {
  it('roundtrips all numeric values through mapping helpers', () => {
    Object.values(QuoteStatusEnum)
      .filter((v) => typeof v === 'number')
      .forEach((val) => {
        const num = val as number;
        const str = quoteStatusFromApi(num);
        expect(quoteStatusToApi(str)).toBe(num);
      });
  });

  it('defaults unknown API value to Draft and unknown string to Draft numeric', () => {
    expect(quoteStatusFromApi(999)).toBe(QuoteStatus.Draft);
    // @ts-expect-error intentionally passing invalid enum
    expect(quoteStatusToApi('foobar')).toBe(QuoteStatusEnum.Draft);
  });
});

describe('OrderStatus numeric enum completeness', () => {
  it('contains every status defined in the numeric enum, matching backend CRM.Core.Entities.OrderStatus', () => {
    // compile-time presence check against actual numeric enum
    expect(OrderStatus.Draft).toBe(0);
    expect(OrderStatus.PendingApproval).toBe(1);
    expect(OrderStatus.Approved).toBe(2);
    expect(OrderStatus.Processing).toBe(3);
    expect(OrderStatus.PartiallyFulfilled).toBe(4);
    expect(OrderStatus.Fulfilled).toBe(5);
    expect(OrderStatus.Delivered).toBe(6);
    expect(OrderStatus.Completed).toBe(7);
    expect(OrderStatus.Cancelled).toBe(8);
    expect(OrderStatus.Returned).toBe(9);
    expect(OrderStatus.Refunded).toBe(10);
    expect(OrderStatus.OnHold).toBe(11);
    expect(OrderStatus.ActionRequired).toBe(12);
  });
});
