-- =============================================================================
-- SYS-011: Customer Satisfaction Tracking (CSAT / NPS / CES)
-- Creates SatisfactionSurveys and SatisfactionResponses tables.
-- SurveyType: 0=CSAT, 1=NPS, 2=CES
-- SurveyStatus: 0=Pending, 1=Sent, 2=Responded, 3=Expired, 4=Cancelled
-- SentimentType: 0=VeryPositive, 1=Positive, 2=Neutral, 3=Negative, 4=VeryNegative
-- =============================================================================


SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
SET time_zone = '+00:00';

CREATE TABLE IF NOT EXISTS SatisfactionSurveys (
    Id                  INT            NOT NULL AUTO_INCREMENT,
    EntityType          VARCHAR(100)   NOT NULL,
    EntityId            INT            NOT NULL,
    Type                INT            NOT NULL DEFAULT 0,   -- SurveyType enum
    Status              INT            NOT NULL DEFAULT 0,   -- SurveyStatus enum
    ContactId           INT            NULL,
    AccountId           INT            NULL,
    SentAt              DATETIME(6)    NULL,
    ResponseReceivedAt  DATETIME(6)    NULL,
    ExpiresAt           DATETIME(6)    NULL,
    ExternalToken       VARCHAR(64)    NULL,
    Subject             VARCHAR(255)   NULL,
    CreatedAt           DATETIME(6)    NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt           DATETIME(6)    NULL     ON UPDATE CURRENT_TIMESTAMP(6),
    IsDeleted           TINYINT(1)     NOT NULL DEFAULT 0,
    RowVersion          TIMESTAMP(6)   NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE INDEX UX_SatisfactionSurveys_ExternalToken (ExternalToken),
    INDEX IX_SatisfactionSurveys_EntityType_EntityId  (EntityType, EntityId),
    INDEX IX_SatisfactionSurveys_ContactId            (ContactId),
    INDEX IX_SatisfactionSurveys_AccountId            (AccountId),
    CONSTRAINT FK_SatisfactionSurveys_Contact FOREIGN KEY (ContactId)
        REFERENCES Contacts (Id) ON DELETE SET NULL,
    CONSTRAINT FK_SatisfactionSurveys_Account FOREIGN KEY (AccountId)
        REFERENCES Customers (Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS SatisfactionResponses (
    Id           INT           NOT NULL AUTO_INCREMENT,
    SurveyId     INT           NOT NULL,
    Score        INT           NOT NULL,
    Comment      TEXT          NULL,
    Sentiment    INT           NOT NULL DEFAULT 2,   -- SentimentType enum (2=Neutral)
    IpAddress    VARCHAR(45)   NULL,
    UserAgent    VARCHAR(512)  NULL,
    RespondedAt  DATETIME(6)   NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CreatedAt    DATETIME(6)   NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt    DATETIME(6)   NULL     ON UPDATE CURRENT_TIMESTAMP(6),
    IsDeleted    TINYINT(1)    NOT NULL DEFAULT 0,
    RowVersion   TIMESTAMP(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE INDEX UX_SatisfactionResponses_SurveyId (SurveyId),
    CONSTRAINT FK_SatisfactionResponses_Survey FOREIGN KEY (SurveyId)
        REFERENCES SatisfactionSurveys (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;