
CREATE TABLE master_Training (
    code VARCHAR(20) NOT NULL,
    titleName NVARCHAR(255) NULL,
    createdBy NVARCHAR(100) NULL,
    createdDate DATETIME NOT NULL  CONSTRAINT DF_master_Training_createdDate DEFAULT (GETDATE()),
    modifiedBy NVARCHAR(100) NULL,
    modifiedDate DATETIME NULL,
    CONSTRAINT PK_master_Training PRIMARY KEY (code)
);

CREATE TABLE dbo.empTraining
(
    UID INT IDENTITY(1,1) PRIMARY KEY,
    empcode NVARCHAR(100) NOT NULL,
    trainingAttended NVARCHAR(100) NULL,
    programme NVARCHAR(100) NULL,
    remarks NVARCHAR(500) NULL,
    markScored NVARCHAR(100) NULL,
    dateAttended DATETIME NULL,
    hours NVARCHAR(100) NULL,
    trainingAttachmentURL NVARCHAR(MAX) NULL,
    sectionCode NVARCHAR(100) NULL,
    method NVARCHAR(100) NULL,
    type NVARCHAR(100) NULL,
    trainerCode NVARCHAR(100) NULL,
    trainerName NVARCHAR(100) NULL,

    createdBy NVARCHAR(100) NULL,
    createdDate DATETIME NOT NULL DEFAULT GETDATE(),
    modifiedBy NVARCHAR(100) NULL,
    modifiedDate DATETIME NULL,
    isActive BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_empTraining_empMaster
        FOREIGN KEY (empcode)
        REFERENCES EHRM.dbo.empMaster(empCode)
);



