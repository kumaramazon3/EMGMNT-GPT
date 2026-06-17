USE [EHRM]
GO
/****** Object:  Trigger [dbo].[trg_empMaster_Log]    Script Date: 6/5/2026 7:27:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER TRIGGER [dbo].[trg_empMaster_Log]
ON [EHRM].[dbo].[empMaster]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO EHRM.dbo.logempMaster
    (
        recNo,
        empCode,
        empName,
        dateOfBirth,
        gender,
        dateOfJoin,
        contractperiod,
        empType,
        nationality,
        isForeign,
        bloodGroup,
        icNo,
        icFilePath,
        emailID,
        mobileNo,
        religion,
        race,
        maritalStatus,
        spouseName,
        spouseWorking,
        noOfChildren,
        permanentAddress,
        currentAddress,
        dateOfService,
        leaveLevel,
        dateOfTerimination,
        epf,
        sosco,
        taxNo,
        workingExp,
        others,
        empCodeI,
        photo,
        isResigned,
        isOperator,
        emergencyContactPerson,
        emergencyAddress,
        emergencyContact,
        relation,
        createdBy,
        createdDate,
        modifiedBy,
        modifiedDate,
        department,
        subDepartment,
        section,
        designation,
        isStayingHostel,
        hostelName,
        houseNo,
        isTransport,
        route,
        isDrivingLicense,
        License,
        vehicleNo1,
        vehicleNo2,
        vehicleNo3,
        bankName,
        accountNumber,
        educationLevel,
        languageProficiency,
        proficiencyLevel,
        fieldOfStudy,
        professionalDocurl,
        otherQualifications,
        resumeDocURL,
        academicCertificateUrl,
        otherCertificateUrl,
        passportNo,
        passportfileurl,
        pwd,
        ActionType
    )
    SELECT
        i.*,
        CASE
            WHEN i.modifiedBy IS NOT NULL THEN 'UPDATE'
            WHEN i.createdBy IS NOT NULL THEN 'INSERT'
            ELSE 'INSERT'
        END AS ActionType
    FROM inserted i;
END;




USE [EHRM]
GO
/****** Object:  Trigger [dbo].[trg_EmpMaster_Sync_To_Old]    Script Date: 6/5/2026 7:28:41 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER TRIGGER [dbo].[trg_EmpMaster_Sync_To_Old]
ON [EHRM].[dbo].[empMaster]
AFTER INSERT--, UPDATE
AS

BEGIN
    SET NOCOUNT ON;
    MERGE Hrmis.dbo.empmaster AS TARGET
    USING
    (
        SELECT
            empcode,
            empName,
            designation,
            section        AS sectioncode,
           --department     AS departmentcode,     --> Old not working in Payment scenario wise (Date: 12-Mar-2026) by gANDi
            subDepartment     AS departmentcode,	--> That's why we need to use sub-Department as a Department (while inserting into Old Hrmis.dbo.empMaster table)
            emailID        AS email,
            dateOfJoin     AS dateofjoin,
            empType        AS emptype,
            gender         AS sex,
            isForeign      AS foreignemp,
            dateOfBirth    AS dateofbirth,
            icNo           AS icno,
            passportNo     AS passportno,
            isResigned     AS resigned,
            permanentAddress AS address1,
            LEFT(currentAddress, 40) AS address2,
            isStayingHostel  AS stayinhostel,
            isTransport      AS transportneeded,
            route,
            race,
            religion,
            mobileNo      AS pphone,
            bloodGroup    AS bloodgroup,
            nationality,
            maritalStatus AS maritalstatus,
            noOfChildren  AS noofchildren,
            epf,
            sosco,
            taxNo,
            educationLevel AS edulevel,
            others,
            contractperiod AS contract,
            hostelName     AS hostelname,
            accountNumber  AS accountno,
            bankName       AS bank,
            isOperator,
            dateOfService  AS DATEOFSERVICE,
            emergencyContactPerson AS EmergencyContactPerson,
            emergencyAddress       AS EmergencyAddress,
            emergencyContact       AS EmergencyTelephone,
            
            createdDate            AS createdtime,
            pwd
        FROM INSERTED
    ) AS SOURCE
    ON TARGET.empcode = SOURCE.empcode

    -- Update existing records
    WHEN MATCHED THEN
        UPDATE SET
            TARGET.empname = SOURCE.empName,
            --TARGET.designation = SOURCE.designation,
            TARGET.designation = COALESCE(NULLIF(SOURCE.designation, ''), TARGET.designation),

            TARGET.sectioncode = SOURCE.sectioncode,
           -- TARGET.departmentcode = SOURCE.departmentcode,
            TARGET.departmentcode = COALESCE(NULLIF(SOURCE.departmentcode, ''), TARGET.departmentcode),

            TARGET.email = SOURCE.email,
            TARGET.dateofjoin = SOURCE.dateofjoin,
            TARGET.emptype = SOURCE.emptype,
            TARGET.sex = SOURCE.sex,
            TARGET.foreignemp = SOURCE.foreignemp,
            TARGET.dateofbirth = SOURCE.dateofbirth,
            TARGET.newicno = SOURCE.icno,
            TARGET.passportno = SOURCE.passportno,
            TARGET.resigned = SOURCE.resigned,
            TARGET.address1 = SOURCE.address1,
            TARGET.address2 = SOURCE.address2,
            TARGET.stayinhostel = SOURCE.stayinhostel,
            TARGET.transportneeded = SOURCE.transportneeded,
            TARGET.route = SOURCE.route,
            TARGET.race = SOURCE.race,
            TARGET.religion = SOURCE.religion,
            TARGET.pphone = SOURCE.pphone,
            TARGET.bloodgroup = SOURCE.bloodgroup,
            TARGET.nationality = SOURCE.nationality,
            TARGET.maritalstatus = SOURCE.maritalstatus,
            TARGET.noofchildren = SOURCE.noofchildren,
            TARGET.epf = SOURCE.epf,
            TARGET.sosco = SOURCE.sosco,
            TARGET.taxNo = SOURCE.taxNo,
            TARGET.edulevel = SOURCE.edulevel,
            TARGET.others = SOURCE.others,
            TARGET.contract = SOURCE.contract,
            TARGET.hostelname = SOURCE.hostelname,
            TARGET.accountno = SOURCE.accountno,
            TARGET.bank = SOURCE.bank,
            TARGET.IsOperator = SOURCE.IsOperator,
            TARGET.DATEOFSERVICE = SOURCE.DATEOFSERVICE,
            TARGET.EmergencyContactPerson = SOURCE.EmergencyContactPerson,
            TARGET.EmergencyAddress = SOURCE.EmergencyAddress,
            TARGET.EmergencyTelephone = SOURCE.EmergencyTelephone,
            TARGET.modifiedtime = GETDATE()  -- ✅ Set modified date on update
            -- pwd and createdtime are NOT updated

    -- Insert new records
    WHEN NOT MATCHED BY TARGET THEN
        INSERT
        (
            empcode, empname, designation, sectioncode, departmentcode,
            email, dateofjoin, emptype, sex, foreignemp,
            dateofbirth, newicno, passportno, resigned,
            address1, address2, stayinhostel, transportneeded,
            route, race, religion, pphone, bloodgroup,
            nationality, maritalstatus, noofchildren,
            epf, sosco, taxNo, edulevel,
            others, contract, hostelname, accountno, bank,
            IsOperator, DATEOFSERVICE,
            EmergencyContactPerson, EmergencyAddress, EmergencyTelephone,
            createdtime, pwd
        )
        VALUES
        (
            SOURCE.empcode, SOURCE.empName, SOURCE.designation,
            SOURCE.sectioncode, SOURCE.departmentcode,
            SOURCE.email, SOURCE.dateofjoin, SOURCE.emptype,
            SOURCE.sex, SOURCE.foreignemp,
            SOURCE.dateofbirth, SOURCE.icno, SOURCE.passportno,
            SOURCE.resigned, SOURCE.address1, SOURCE.address2,
            SOURCE.stayinhostel, SOURCE.transportneeded,
            SOURCE.route, SOURCE.race, SOURCE.religion,
            SOURCE.pphone, SOURCE.bloodgroup, SOURCE.nationality,
            SOURCE.maritalstatus, SOURCE.noofchildren,
            SOURCE.epf, SOURCE.sosco, SOURCE.taxNo,
            SOURCE.edulevel, SOURCE.others, SOURCE.contract,
            SOURCE.hostelname, SOURCE.accountno, SOURCE.bank,
            SOURCE.IsOperator, SOURCE.DATEOFSERVICE,
            SOURCE.EmergencyContactPerson, SOURCE.EmergencyAddress,
            SOURCE.EmergencyTelephone,
            SOURCE.createdtime, SOURCE.pwd
        );

END;




USE [EHRM]
GO
/****** Object:  Trigger [dbo].[trg_empmaster_update_leavelevel]    Script Date: 6/5/2026 7:28:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER TRIGGER [dbo].[trg_empmaster_update_leavelevel]
ON [EHRM].[dbo].[empMaster]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Only proceed if 'designation' column was updated
    IF UPDATE(designation)
    BEGIN
        UPDATE e
        SET e.leavelevel = ISNULL(d.dlevel, 0)  -- If no match, set 0
        FROM EHRM.dbo.empmaster e
        INNER JOIN inserted i ON e.empCode = i.empCode
        LEFT JOIN EHRM.dbo.master_Designation d 
            ON d.designationname = i.designation;
    END
END;
