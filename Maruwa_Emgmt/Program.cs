using log4net;
using log4net.Config;
using Maruwa_Emgmt.BAL;
using Maruwa_Emgmt.BAL.Leave;
using Maruwa_Emgmt.BAL.master;
using Maruwa_Emgmt.BAL.SkillMatrix;
using Maruwa_Emgmt.BAL.Training;
using Maruwa_Emgmt.DAL;
using Maruwa_Emgmt.DAL.Leave;
using Maruwa_Emgmt.DAL.master;
using Maruwa_Emgmt.DAL.SkillMatrix;
using Maruwa_Emgmt.DAL.Training;
using Maruwa_Emgmt.DBcontex;
using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.InterFace.Leave;
using Maruwa_Emgmt.InterFace.master;
using Maruwa_Emgmt.InterFace.SkillMatrix;
using Maruwa_Emgmt.InterFace.Training;
using Maruwa_Emgmt.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

#region ------------------- Configure log4net ------------------- (Debug/Bin/---)
var logPath = Path.Combine(AppContext.BaseDirectory, "Logs");// Create Logs folder in output directory if it doesn't exist
if (!Directory.Exists(logPath))
{
    Directory.CreateDirectory(logPath);
}
log4net.GlobalContext.Properties["LogPath"] = logPath;// Set log4net property for dynamic path
#endregion

// Configure log4net using config file in DBcontex folder
var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo(Path.Combine(AppContext.BaseDirectory, "DBcontex", "log4net.config")));

// Get logger instance
var logger = LogManager.GetLogger(typeof(Program));

// ------------------- Services Configuration -------------------

// Add services to the container
builder.Services.AddControllersWithViews();

// ------------------- Session Configuration -------------------
builder.Services.AddDistributedMemoryCache(); // Stores Login user information in session and then that Session information store in Server Side Memory.

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);  // Session timeout
    options.Cookie.HttpOnly = true;  // Prevent client-side access  // Security
    options.Cookie.IsEssential = true; // Required for GDPR compliance
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// ------------------- Connection String -------------------
var connectionString = builder.Configuration.GetConnectionString("EHRMConnection"); // Local Connection String
var liveConn = builder.Configuration.GetConnectionString("LiveHRMISConnection");// Local with same Columns in Table
builder.Services.Configure<EmpDesignationSettings>(builder.Configuration.GetSection("EmpDesignation"));

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDbContext<LiveHRMISDbContext>(options => options.UseSqlServer(liveConn));

#region ------------------- Dependency Injection Setup (Register DAL & Services) -------------------

#region Employee Registration

builder.Services.AddScoped<i_tblempmaster, da_tblempmaster>();
builder.Services.AddScoped<bal_tblempmaster>();
builder.Services.AddScoped<i_tbldropdownData, da_tbldropdownData>();
builder.Services.AddScoped<bal_tbldropdownData>();
builder.Services.AddScoped<IFtpService, da_FtpService>();
builder.Services.AddScoped<IProjectLocationService, da_ProjectLocationService>();

#endregion

#region Training Module
builder.Services.AddScoped<i_trainingMaster, da_trainingMaster>();// DAL
builder.Services.AddScoped<bll_trainingMaster>();// BLL

builder.Services.AddScoped<i_empTraining, da_empTraining>();// DAL
builder.Services.AddScoped<bll_empTraining>();// BLL

builder.Services.AddScoped<i_trainingList, dal_trainingList>();// DAL
builder.Services.AddScoped<bll_trainingList>();// BLL

builder.Services.AddScoped<i_trainingProgram, dal_trainingProgram>();// DAL
builder.Services.AddScoped<bll_trainingProgram>();// BLL

builder.Services.AddScoped<i_tna, dal_tna>();// DAL
builder.Services.AddScoped<bll_tna>();// BLL

#endregion

#region Skill Matrix
builder.Services.AddScoped<i_SkillMatrix, dal_SkillMatrix>();// DAL
builder.Services.AddScoped<bll_skmdata>();// BLL

#endregion

#region Leaves
builder.Services.AddScoped<i_leave, da_leave>();// DAL
builder.Services.AddScoped<bll_leave>();// BLL
#endregion


#region Master
builder.Services.AddScoped<i_Designation, da_Designation>();// DAL
builder.Services.AddScoped<bll_Designation>();// BLL
builder.Services.AddScoped<i_DepartmentMaster, da_DepartmentMaster>();// ADO.NET DAL
builder.Services.AddScoped<bll_DepartmentMaster>();// BLL
builder.Services.AddScoped<i_SectionMaster, da_SectionMaster>();// ADO.NET DAL
builder.Services.AddScoped<bll_SectionMaster>();// BLL



#endregion

#endregion


// ------------------- Logging Configuration -------------------
builder.Logging.ClearProviders();// removes defaults if you want full control
builder.Logging.AddLog4Net(Path.Combine("DBcontex", "log4net.config"));

// ------------------- Build the app -------------------
var app = builder.Build();
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!db.Database.CanConnect())
    {
        logger.Error("Cannot connect to the database. Please check the connection string and SQL Server permissions.");
        throw new Exception("Database not accessible.");
    }
    else
    {
        logger.Info("Database connection successful.");
    }
}
catch (Exception ex)
{
    logger.Error("Exception while validating database connection.", ex);
    // Optional: Stop app or continue depending on requirement
}

// ------------------- Configure HTTP request pipeline -------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseSession();// Use session before routing
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(name: "default",pattern: "{controller=LoginUser}/{action=Login}/{id?}");
app.Run();
