using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.Leave;
using Maruwa_Emgmt.Models.master;
using Maruwa_Emgmt.Models.SkillMatrix;
using Maruwa_Emgmt.Models.TM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Maruwa_Emgmt.DBcontex
{
    public class ApplicationDbContext : DbContext
    {
        // Default constructor that accepts only DbContextOptions
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            // The DbContextOptions passed here already include the connection string
        }

        // DbSets for your models
        public DbSet<tblempmaster> loginempmaster { get; set; }
        public DbSet<tblempmaster> tblempmaster { get; set; }
        public DbSet<empMaster> empmaster { get; set; }
        public DbSet<uniformInfo> uniforminfo { get; set; }
        public DbSet<accessoriesInfo> accessoriesinfo { get; set; }
        public DbSet<tbldropdownData> tbldropdownData { get; set; }
        public DbSet<tblsections> tblsections { get; set; }
        public DbSet<tblhousenumber> tblhousenumber { get; set; }
        public DbSet<tbldropdownData> tbllicensetypes { get; set; }

        #region Training
        public DbSet<trainingProgram> trainingprogram { get; set; }
        public DbSet<master_Skill_Matrix> masterskillmatrix { get; set; }
        public DbSet<new_skillMatrixDocumentData> new_masterskillmatrix { get; set; }
        public DbSet<SelectskillMatrixDocumentData> getskillmatrixasync { get; set; }
        public DbSet<OriginalReport_skillMatrixDocumentData> original_masterskillmatrix { get; set; }
        public DbSet<skillMatrixDocumentData_log> skillmatrixlog_years { get; set; }
        public DbSet<skillMatrixReportList> masterskillmatrixReportList { get; set; }

        public DbSet<skillMatrixReportByDocNo> skillMatrixReportByDocNo { get; set; }

        #endregion


        #region Leaves
        public DbSet<leaveform> leaveform { get; set; }

        #endregion

        #region Master
        public DbSet<Models.master_Designation> designation { get; set; }
        public DbSet<skillMatrixDocumentData> skillmatrixdocumentdata { get; set; }
        public DbSet<Insert_skillMatrix> Insertskillmatrixdata { get; set; }


        #endregion

        // OnModelCreating is overridden to configure models and relationships (optional but useful)

        #region all Drop Down DB-context
        public DbSet<master_Nationality> master_nationality { get; set; }
        public DbSet<master_BloodGroup> master_bloodgroup { get; set; }
        public DbSet<master_Bank> master_bank { get; set; }
        public DbSet<master_Department> master_department { get; set; }
        public DbSet<master_Education> master_education { get; set; }
        public DbSet<master_Hostal> master_hostal { get; set; }
        public DbSet<master_Language> master_language { get; set; }
        public DbSet<master_License> master_license { get; set; }
        public DbSet<master_Race> master_race { get; set; }
        public DbSet<master_Religion> master_religion { get; set; }
        public DbSet<master_Route> master_route { get; set; }
        public DbSet<master_SubDepartment> master_subdepartment { get; set; }
        public DbSet<master_DepartmentSection> master_departmentsection { get; set; }
        public DbSet<Models.master_Designation> master_designation { get; set; }
        public DbSet<master_EmployeeType> master_employeetype { get; set; }

        public DbSet<trainingMaster> trainingmaster { get; set; }
        public DbSet<trainingList> traininglist { get; set; }
        public DbSet<master_LeaveType> leavetype { get; set; }
        public DbSet<LeaveTiming> LeaveTime { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<skillMatrixDocumentData_log>().ToTable("skillMatrixDocumentData_log", "dbo");
            modelBuilder.Entity<SectionCodeModel>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // SP result only, no table/view
            });
            base.OnModelCreating(modelBuilder);
        }
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    // Model configurations can go here (if needed)
        //    base.OnModelCreating(modelBuilder);

        //    // Map entity to actual table in DB
        //    modelBuilder.Entity<skillMatrixDocumentData_log>().ToTable("skillMatrixDocumentData_log", "dbo"); // schema = EHRM
        //}

    }
}
