-- =============================================================================
-- SYS-010: Record Comments & @Mentions
-- Creates ThreadedComments table for entity-agnostic comment threads.
-- Supports @mentions via JSON user-ID array in MentionedUserIds column.
-- =============================================================================


SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
SET time_zone = '+00:00';

CREATE TABLE IF NOT EXISTS RecordComments (
    Id                 INT            NOT NULL AUTO_INCREMENT,
    EntityType         VARCHAR(50)    NOT NULL,
    EntityId           INT            NOT NULL,
    Content            VARCHAR(4000)  NOT NULL,
    AuthorId           INT            NOT NULL,
    ParentCommentId    INT            NULL,
    MentionedUserIds   TEXT           NULL,
    CreatedAt          DATETIME(6)    NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt          DATETIME(6)    NULL     ON UPDATE CURRENT_TIMESTAMP(6),
    IsDeleted          TINYINT(1)     NOT NULL DEFAULT 0,
    RowVersion         TIMESTAMP(6)   NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    INDEX IX_RecordComments_Entity  (EntityType, EntityId),
    INDEX IX_RecordComments_Author  (AuthorId),
    INDEX IX_RecordComments_Parent  (ParentCommentId),
    CONSTRAINT FK_RecordComments_Author FOREIGN KEY (AuthorId)
        REFERENCES Users (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_RecordComments_Parent FOREIGN KEY (ParentCommentId)
        REFERENCES RecordComments (Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;