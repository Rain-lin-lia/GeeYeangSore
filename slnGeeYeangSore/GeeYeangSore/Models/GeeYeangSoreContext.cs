using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GeeYeangSore.Models;

public partial class GeeYeangSoreContext : DbContext
{
    public GeeYeangSoreContext()
    {
    }

    public GeeYeangSoreContext(DbContextOptions<GeeYeangSoreContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<HAbout> HAbouts { get; set; }

    public virtual DbSet<HAd> HAds { get; set; }

    public virtual DbSet<HAdPlan> HAdPlans { get; set; }

    public virtual DbSet<HAdmin> HAdmins { get; set; }

    public virtual DbSet<HAdminLog> HAdminLogs { get; set; }

    public virtual DbSet<HAudit> HAudits { get; set; }

    public virtual DbSet<HChat> HChats { get; set; }

    public virtual DbSet<HContact> HContacts { get; set; }

    public virtual DbSet<HEmailToken> HEmailTokens { get; set; }

    public virtual DbSet<HFavorite> HFavorites { get; set; }

    public virtual DbSet<HGuide> HGuides { get; set; }

    public virtual DbSet<HLandlord> HLandlords { get; set; }

    public virtual DbSet<HLblacklist> HLblacklists { get; set; }

    public virtual DbSet<HMatch> HMatches { get; set; }

    public virtual DbSet<HMblacklist> HMblacklists { get; set; }

    public virtual DbSet<HMessage> HMessages { get; set; }

    public virtual DbSet<HNews> HNews { get; set; }

    public virtual DbSet<HNotify> HNotifies { get; set; }

    public virtual DbSet<HPasswordReset> HPasswordResets { get; set; }

    public virtual DbSet<HPost> HPosts { get; set; }

    public virtual DbSet<HPostMonitoring> HPostMonitorings { get; set; }

    public virtual DbSet<HProperty> HProperties { get; set; }

    public virtual DbSet<HPropertyAudit> HPropertyAudits { get; set; }

    public virtual DbSet<HPropertyFeature> HPropertyFeatures { get; set; }

    public virtual DbSet<HPropertyImage> HPropertyImages { get; set; }

    public virtual DbSet<HReaction> HReactions { get; set; }

    public virtual DbSet<HReply> HReplies { get; set; }

    public virtual DbSet<HReport> HReports { get; set; }

    public virtual DbSet<HReportForum> HReportForums { get; set; }

    public virtual DbSet<HRevenueReport> HRevenueReports { get; set; }

    public virtual DbSet<HSso> HSsos { get; set; }

    public virtual DbSet<HTenant> HTenants { get; set; }

    public virtual DbSet<HTransaction> HTransactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=26.135.207.98;Initial Catalog=GeeYeangSore;User ID=admin01;Password=admin01;Encrypt=False;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<HAbout>(entity =>
        {
            entity.ToTable("h_About");

            entity.Property(e => e.HAboutId).HasColumnName("h_About_Id");
            entity.Property(e => e.HContent).HasColumnName("h_Content");
            entity.Property(e => e.HCreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HTitle).HasColumnName("h_Title");
            entity.Property(e => e.HUpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_UpdatedAt");
        });

        modelBuilder.Entity<HAd>(entity =>
        {
            entity.ToTable("h_Ad");

            entity.Property(e => e.HAdId).HasColumnName("h_Ad_Id");
            entity.Property(e => e.HAdName)
                .HasMaxLength(50)
                .HasColumnName("h_AdName");
            entity.Property(e => e.HAdPrice)
                .HasColumnType("money")
                .HasColumnName("h_AdPrice");
            entity.Property(e => e.HAdTag)
                .HasMaxLength(50)
                .HasColumnName("h_AdTag");
            entity.Property(e => e.HCategory)
                .HasMaxLength(50)
                .HasColumnName("h_Category");
            entity.Property(e => e.HCreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedDate");
            entity.Property(e => e.HDescription).HasColumnName("h_Description");
            entity.Property(e => e.HEndDate)
                .HasColumnType("datetime")
                .HasColumnName("h_EndDate");
            entity.Property(e => e.HImageUrl)
                .HasMaxLength(250)
                .HasColumnName("h_ImageURL");
            entity.Property(e => e.HIsDelete).HasColumnName("h_IsDelete");
            entity.Property(e => e.HLandlordId).HasColumnName("h_Landlord_Id");
            entity.Property(e => e.HLastUpdated)
                .HasColumnType("datetime")
                .HasColumnName("h_LastUpdated");
            entity.Property(e => e.HLinkUrl)
                .HasMaxLength(250)
                .HasColumnName("h_LinkURL");
            entity.Property(e => e.HPlanId).HasColumnName("h_PlanId");
            entity.Property(e => e.HPriority).HasColumnName("h_Priority");
            entity.Property(e => e.HPropertyId).HasColumnName("h_Property_Id");
            entity.Property(e => e.HStartDate)
                .HasColumnType("datetime")
                .HasColumnName("h_StartDate");
            entity.Property(e => e.HStatus)
                .HasMaxLength(50)
                .HasColumnName("h_Status");
            entity.Property(e => e.HTargetRegion)
                .HasMaxLength(50)
                .HasColumnName("h_TargetRegion");

            entity.HasOne(d => d.HLandlord).WithMany(p => p.HAds)
                .HasForeignKey(d => d.HLandlordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_h_Ad_Landlord");

            entity.HasOne(d => d.HPlan).WithMany(p => p.HAds)
                .HasForeignKey(d => d.HPlanId)
                .HasConstraintName("FK_h_Ad_h_AdPlan");

            entity.HasOne(d => d.HProperty).WithMany(p => p.HAds)
                .HasForeignKey(d => d.HPropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_h_Ad_Property");
        });

        modelBuilder.Entity<HAdPlan>(entity =>
        {
            entity.HasKey(e => e.HPlanId);

            entity.ToTable("h_AdPlan");

            entity.Property(e => e.HPlanId).HasColumnName("h_PlanId");
            entity.Property(e => e.HAdPrice).HasColumnName("h_AdPrice");
            entity.Property(e => e.HCategory)
                .HasMaxLength(50)
                .HasColumnName("h_Category");
            entity.Property(e => e.HDays).HasColumnName("h_Days");
            entity.Property(e => e.HName)
                .HasMaxLength(50)
                .HasColumnName("h_Name");
        });

        modelBuilder.Entity<HAdmin>(entity =>
        {
            entity.HasKey(e => e.HAdminId).HasName("PK__h_Admin__095E25859F518A03");

            entity.ToTable("h_Admin");

            entity.HasIndex(e => e.HAccount, "UQ_h_Admin_Account").IsUnique();

            entity.Property(e => e.HAdminId).HasColumnName("h_Admin_Id");
            entity.Property(e => e.HAccount)
                .HasMaxLength(100)
                .HasColumnName("h_Account");
            entity.Property(e => e.HCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HIsDeleted).HasColumnName("h_IsDeleted");
            entity.Property(e => e.HPassword)
                .HasMaxLength(225)
                .HasColumnName("h_Password");
            entity.Property(e => e.HRoleLevel)
                .HasMaxLength(50)
                .HasColumnName("h_RoleLevel");
            entity.Property(e => e.HSalt)
                .HasMaxLength(100)
                .HasColumnName("h_Salt");
            entity.Property(e => e.HUpdateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("h_UpdateAt");
        });

        modelBuilder.Entity<HAdminLog>(entity =>
        {
            entity.HasKey(e => e.HAdminLogId).HasName("PK__h_AdminL__79DB54DCBA6DD6DD");

            entity.ToTable("h_AdminLog");

            entity.Property(e => e.HAdminLogId).HasColumnName("h_AdminLog_Id");
            entity.Property(e => e.HAdminId).HasColumnName("h_Admin_Id");
            entity.Property(e => e.HCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HDescription)
                .HasMaxLength(100)
                .HasColumnName("h_Description");
            entity.Property(e => e.HOperationType)
                .HasMaxLength(50)
                .HasColumnName("h_OperationType");

            entity.HasOne(d => d.HAdmin).WithMany(p => p.HAdminLogs)
                .HasForeignKey(d => d.HAdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_h_AdminLog_h_Admin");
        });

        modelBuilder.Entity<HAudit>(entity =>
        {
            entity.ToTable("h_Audit");

            entity.Property(e => e.HAuditId).HasColumnName("h_Audit_Id");
            entity.Property(e => e.HBankAccount)
                .HasMaxLength(100)
                .HasColumnName("h_BankAccount");
            entity.Property(e => e.HBankName)
                .HasMaxLength(100)
                .HasColumnName("h_BankName");
            entity.Property(e => e.HIdCardBackPath).HasColumnName("h_IdCardBackPath");
            entity.Property(e => e.HIdCardFrontPath).HasColumnName("h_IdCardFrontPath");
            entity.Property(e => e.HReviewNote)
                .HasMaxLength(500)
                .HasColumnName("h_ReviewNote");
            entity.Property(e => e.HReviewedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_ReviewedAt");
            entity.Property(e => e.HStatus)
                .HasMaxLength(100)
                .HasColumnName("h_Status");
            entity.Property(e => e.HSubmittedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_SubmittedAt");
            entity.Property(e => e.HTenantId).HasColumnName("h_Tenant_Id");
        });

        modelBuilder.Entity<HChat>(entity =>
        {
            entity.HasKey(e => e.HChatId).HasName("PK__h_Chats__1385C565EE6B0017");

            entity.ToTable("h_Chats");

            entity.Property(e => e.HChatId).HasColumnName("h_Chat_Id");
            entity.Property(e => e.HAuthorId).HasColumnName("h_Author_Id");
            entity.Property(e => e.HAuthorType)
                .HasMaxLength(50)
                .HasColumnName("h_AuthorType");
            entity.Property(e => e.HChatName)
                .HasMaxLength(100)
                .HasColumnName("h_ChatName");
            entity.Property(e => e.HChatType)
                .HasMaxLength(100)
                .HasColumnName("h_Chat_Type");
            entity.Property(e => e.HCreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_Created_At");
            entity.Property(e => e.HJoinedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_JoinedAt");
            entity.Property(e => e.HPropertyId).HasColumnName("h_Property_Id");
            entity.Property(e => e.HRole)
                .HasMaxLength(50)
                .HasColumnName("h_Role");
            entity.Property(e => e.HStatus).HasColumnName("h_Status");

            entity.HasOne(d => d.HProperty).WithMany(p => p.HChats)
                .HasForeignKey(d => d.HPropertyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_h_Chats_h_Property");
        });

        modelBuilder.Entity<HContact>(entity =>
        {
            entity.ToTable("h_Contact");

            entity.Property(e => e.HContactId).HasColumnName("h_Contact_Id");
            entity.Property(e => e.HAdminId).HasColumnName("h_Admin_Id");
            entity.Property(e => e.HContent).HasColumnName("h_Content");
            entity.Property(e => e.HCreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HEmail)
                .HasMaxLength(225)
                .HasColumnName("h_Email");
            entity.Property(e => e.HPhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("h_PhoneNumber");
            entity.Property(e => e.HReplyAt)
                .HasColumnType("datetime")
                .HasColumnName("h_ReplyAt");
            entity.Property(e => e.HReplyContent).HasColumnName("h_ReplyContent");
            entity.Property(e => e.HStatus).HasColumnName("h_Status");
            entity.Property(e => e.HTenantId).HasColumnName("h_Tenant_Id");
            entity.Property(e => e.HTitle)
                .HasMaxLength(225)
                .HasColumnName("h_Title");

            entity.HasOne(d => d.HAdmin).WithMany(p => p.HContacts)
                .HasForeignKey(d => d.HAdminId)
                .HasConstraintName("FK_h_Contact_h_Admin");
        });

        modelBuilder.Entity<HEmailToken>(entity =>
        {
            entity.HasKey(e => e.HEmailTokenId).HasName("PK__h_EmailT__1F9832157C7A52B6");

            entity.ToTable("h_EmailToken");

            entity.Property(e => e.HEmailTokenId).HasColumnName("h_EmailToken_Id");
            entity.Property(e => e.HCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HEmailSalt)
                .HasMaxLength(100)
                .HasColumnName("h_EmailSalt");
            entity.Property(e => e.HEmailToken1)
                .HasMaxLength(100)
                .HasColumnName("h_EmailToken");
            entity.Property(e => e.HIsUsed).HasColumnName("h_IsUsed");
            entity.Property(e => e.HRequestIp)
                .HasMaxLength(50)
                .HasColumnName("h_RequestIP");
            entity.Property(e => e.HResetExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("h_ResetExpiresAt");
            entity.Property(e => e.HTokenType)
                .HasMaxLength(50)
                .HasColumnName("h_TokenType");
            entity.Property(e => e.HUsedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_UsedAt");
            entity.Property(e => e.HUserEmail)
                .HasMaxLength(100)
                .HasColumnName("h_UserEmail");
        });

        modelBuilder.Entity<HFavorite>(entity =>
        {
            entity.HasKey(e => e.HFavoriteId).HasName("PK__h_Favori__1961AC1C20351DE9");

            entity.ToTable("h_Favorite");

            entity.Property(e => e.HFavoriteId).HasColumnName("h_Favorite_Id");
            entity.Property(e => e.HCreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HPropertyId).HasColumnName("h_Property_Id");
            entity.Property(e => e.HTenantId).HasColumnName("h_Tenant_Id");

            entity.HasOne(d => d.HProperty).WithMany(p => p.HFavorites)
                .HasForeignKey(d => d.HPropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_h_Favorite_Property");

            entity.HasOne(d => d.HTenant).WithMany(p => p.HFavorites)
                .HasForeignKey(d => d.HTenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_h_Favorite_Tenant");
        });

        modelBuilder.Entity<HGuide>(entity =>
        {
            entity.ToTable("h_Guide");

            entity.Property(e => e.HGuideId).HasColumnName("h_Guide_Id");
            entity.Property(e => e.HContent).HasColumnName("h_Content");
            entity.Property(e => e.HCreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HImagePath).HasColumnName("h_ImagePath");
            entity.Property(e => e.HTitle)
                .HasMaxLength(100)
                .HasColumnName("h_Title");
            entity.Property(e => e.HUpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_UpdatedAt");
        });

        modelBuilder.Entity<HLandlord>(entity =>
        {
            entity.HasKey(e => e.HLandlordId).HasName("PK__h_Landlo__D9DF936B933212BA");

            entity.ToTable("h_Landlord");

            entity.Property(e => e.HLandlordId).HasColumnName("h_Landlord_Id");
            entity.Property(e => e.HBankAccount)
                .HasMaxLength(100)
                .HasColumnName("h_BankAccount");
            entity.Property(e => e.HBankName)
                .HasMaxLength(100)
                .HasColumnName("h_BankName");
            entity.Property(e => e.HCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HIdCardBackUrl).HasColumnName("h_IdCardBackUrl");
            entity.Property(e => e.HIdCardFrontUrl).HasColumnName("h_IdCardFrontUrl");
            entity.Property(e => e.HIsDeleted).HasColumnName("h_IsDeleted");
            entity.Property(e => e.HLandlordName)
                .HasMaxLength(50)
                .HasColumnName("h_LandlordName");
            entity.Property(e => e.HStatus)
                .HasMaxLength(50)
                .HasDefaultValue("未驗證")
                .HasColumnName("h_Status");
            entity.Property(e => e.HTenantId).HasColumnName("h_Tenant_Id");
            entity.Property(e => e.HUpdateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("h_UpdateAt");

            entity.HasOne(d => d.HTenant).WithMany(p => p.HLandlords)
                .HasForeignKey(d => d.HTenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_h_Landlord_Tenant");
        });

        modelBuilder.Entity<HLblacklist>(entity =>
        {
            entity.HasKey(e => e.HLblacklistId).HasName("PK__h_LBlack__01C86407E37950E1");

            entity.ToTable("h_LBlacklist");

            entity.Property(e => e.HLblacklistId).HasColumnName("h_LBlacklist_Id");
            entity.Property(e => e.HAddedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("h_AddedDate");
            entity.Property(e => e.HEntityType)
                .HasMaxLength(50)
                .HasColumnName("h_EntityType");
            entity.Property(e => e.HExpirationDate)
                .HasColumnType("datetime")
                .HasColumnName("h_ExpirationDate");
            entity.Property(e => e.HLandlordId).HasColumnName("h_Landlord_Id");
            entity.Property(e => e.HReason)
                .HasMaxLength(200)
                .HasColumnName("h_Reason");

            entity.HasOne(d => d.HLandlord).WithMany(p => p.HLblacklists)
                .HasForeignKey(d => d.HLandlordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_h_Blacklist_Landlord");
        });

        modelBuilder.Entity<HMatch>(entity =>
        {
            entity.HasKey(e => e.HMatchId).HasName("PK__h_Matche__86626B3FBD2FD31A");

            entity.ToTable("h_Matches");

            entity.Property(e => e.HMatchId).HasColumnName("h_Match_Id");
            entity.Property(e => e.HAcceptsPets).HasColumnName("h_AcceptsPets");
            entity.Property(e => e.HAcceptsSmoking).HasColumnName("h_AcceptsSmoking");
            entity.Property(e => e.HBudget).HasColumnName("h_Budget");
            entity.Property(e => e.HCompatibilityScore).HasColumnName("h_CompatibilityScore");
            entity.Property(e => e.HLastUpdated)
                .HasColumnType("datetime")
                .HasColumnName("h_LastUpdated");
            entity.Property(e => e.HMatchEduserId).HasColumnName("h_MatchEduser_Id");
            entity.Property(e => e.HMatchReason)
                .HasMaxLength(255)
                .HasColumnName("h_MatchReason");
            entity.Property(e => e.HMatchUserIntro)
                .HasMaxLength(500)
                .HasColumnName("h_MatchUserIntro");
            entity.Property(e => e.HMatchdate)
                .HasColumnType("datetime")
                .HasColumnName("h_Matchdate");
            entity.Property(e => e.HPreferreDistrict)
                .HasMaxLength(255)
                .HasColumnName("h_PreferreDistrict");
            entity.Property(e => e.HPreferredCity)
                .HasMaxLength(255)
                .HasColumnName("h_PreferredCity");
            entity.Property(e => e.HPreferredRoommateAge)
                .HasMaxLength(50)
                .HasColumnName("h_PreferredRoommateAge");
            entity.Property(e => e.HPreferredRoommateGender)
                .HasMaxLength(50)
                .HasColumnName("h_PreferredRoommateGender");
            entity.Property(e => e.HPreferredRoommateOccupation)
                .HasMaxLength(50)
                .HasColumnName("h_PreferredRoommateOccupation");
            entity.Property(e => e.HSleepschedule)
                .HasMaxLength(50)
                .HasColumnName("h_Sleepschedule");
            entity.Property(e => e.HStatus)
                .HasMaxLength(50)
                .HasColumnName("h_Status");
            entity.Property(e => e.HTenantId).HasColumnName("h_Tenant_Id");

            entity.HasOne(d => d.HMatchEduser).WithMany(p => p.HMatchHMatchEdusers)
                .HasForeignKey(d => d.HMatchEduserId)
                .HasConstraintName("FK_h_Matches_h_MatchEduser_Id");

            entity.HasOne(d => d.HTenant).WithMany(p => p.HMatchHTenants)
                .HasForeignKey(d => d.HTenantId)
                .HasConstraintName("FK_h_Matches_h_Tenant_Id");
        });

        modelBuilder.Entity<HMblacklist>(entity =>
        {
            entity.HasKey(e => e.HＭblacklistId).HasName("PK__h_Blackl__1EC3D4689EF68D39");

            entity.ToTable("h_MBlacklist");

            entity.Property(e => e.HＭblacklistId).HasColumnName("h_ＭBlacklist_Id");
            entity.Property(e => e.HAddedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("h_AddedDate");
            entity.Property(e => e.HEntityType)
                .HasMaxLength(50)
                .HasColumnName("h_EntityType");
            entity.Property(e => e.HExpirationDate)
                .HasColumnType("datetime")
                .HasColumnName("h_ExpirationDate");
            entity.Property(e => e.HReason)
                .HasMaxLength(225)
                .HasColumnName("h_Reason");
            entity.Property(e => e.HTenantId).HasColumnName("h_Tenant_Id");

            entity.HasOne(d => d.HTenant).WithMany(p => p.HMblacklists)
                .HasForeignKey(d => d.HTenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_h_Blacklist_Tenant");
        });

        modelBuilder.Entity<HMessage>(entity =>
        {
            entity.HasKey(e => e.HMessageId).HasName("PK__h_Messag__FEC43A40167D878F");

            entity.ToTable("h_Messages");

            entity.Property(e => e.HMessageId).HasColumnName("h_Message_Id");
            entity.Property(e => e.HAttachmentUrl)
                .HasMaxLength(255)
                .HasColumnName("h_AttachmentUrl");
            entity.Property(e => e.HChatId).HasColumnName("h_Chat_Id");
            entity.Property(e => e.HContent)
                .HasMaxLength(500)
                .HasColumnName("h_Content");
            entity.Property(e => e.HDeletedByReceiver).HasColumnName("h_DeletedByReceiver");
            entity.Property(e => e.HDeletedBySender).HasColumnName("h_DeletedBySender");
            entity.Property(e => e.HEditedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_EditedAt");
            entity.Property(e => e.HIsDeleted).HasColumnName("h_IsDeleted");
            entity.Property(e => e.HIsEdited).HasColumnName("h_IsEdited");
            entity.Property(e => e.HIsRead).HasColumnName("h_IsRead");
            entity.Property(e => e.HMessageType)
                .HasMaxLength(50)
                .HasColumnName("h_MessageType");
            entity.Property(e => e.HPropertyId).HasColumnName("h_PropertyId");
            entity.Property(e => e.HReceiverId).HasColumnName("h_Receiver_Id");
            entity.Property(e => e.HReceiverRole)
                .HasMaxLength(50)
                .HasColumnName("h_ReceiverRole");
            entity.Property(e => e.HReplyToMessageId).HasColumnName("h_ReplyToMessage_Id");
            entity.Property(e => e.HReportCount).HasColumnName("h_ReportCount");
            entity.Property(e => e.HSenderId).HasColumnName("h_Sender_Id");
            entity.Property(e => e.HSenderRole)
                .HasMaxLength(50)
                .HasColumnName("h_SenderRole");
            entity.Property(e => e.HSource)
                .HasMaxLength(50)
                .HasColumnName("h_Source");
            entity.Property(e => e.HTimestamp)
                .HasColumnType("datetime")
                .HasColumnName("h_Timestamp");

            entity.HasOne(d => d.HChat).WithMany(p => p.HMessages)
                .HasForeignKey(d => d.HChatId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_h_Messages_h_Chats");

            entity.HasOne(d => d.HProperty).WithMany(p => p.HMessages)
                .HasForeignKey(d => d.HPropertyId)
                .HasConstraintName("FK_h_Messages_Property");
        });

        modelBuilder.Entity<HNews>(entity =>
        {
            entity.ToTable("h_News");

            entity.Property(e => e.HNewsId).HasColumnName("h_News_Id");
            entity.Property(e => e.HContent).HasColumnName("h_Content");
            entity.Property(e => e.HCreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HImagePath).HasColumnName("h_ImagePath");
            entity.Property(e => e.HTitle).HasColumnName("h_Title");
            entity.Property(e => e.HUpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_UpdatedAt");
        });

        modelBuilder.Entity<HNotify>(entity =>
        {
            entity.HasKey(e => e.HNotifyId).HasName("PK__h_Notify__82FA3CC3E4D75F4C");

            entity.ToTable("h_Notify");

            entity.Property(e => e.HNotifyId).HasColumnName("h_Notify_Id");
            entity.Property(e => e.HContent)
                .HasMaxLength(50)
                .HasColumnName("h_Content");
            entity.Property(e => e.HCreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HStatus).HasColumnName("h_Status");
            entity.Property(e => e.HTenantId).HasColumnName("h_Tenant_Id");
            entity.Property(e => e.HTitle)
                .HasMaxLength(50)
                .HasColumnName("h_Title");
            entity.Property(e => e.HType)
                .HasMaxLength(50)
                .HasColumnName("h_Type");

            entity.HasOne(d => d.HTenant).WithMany(p => p.HNotifies)
                .HasForeignKey(d => d.HTenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_h_Notify_Tenant");
        });

        modelBuilder.Entity<HPasswordReset>(entity =>
        {
            entity.HasKey(e => e.HPasswordResetId).HasName("PK__h_Passwo__F3E3CDE04B2E5C06");

            entity.ToTable("h_PasswordReset");

            entity.Property(e => e.HPasswordResetId).HasColumnName("h_PasswordReset_Id");
            entity.Property(e => e.HCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HIsUsed).HasColumnName("h_IsUsed");
            entity.Property(e => e.HRequestIp)
                .HasMaxLength(50)
                .HasColumnName("h_RequestIP");
            entity.Property(e => e.HResetExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("h_ResetExpiresAt");
            entity.Property(e => e.HResetToken)
                .HasMaxLength(100)
                .HasColumnName("h_ResetToken");
            entity.Property(e => e.HTenantId).HasColumnName("h_Tenant_Id");
            entity.Property(e => e.HUsedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_UsedAt");

            entity.HasOne(d => d.HTenant).WithMany(p => p.HPasswordResets)
                .HasForeignKey(d => d.HTenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_h_PasswordReset_h_Tenant");
        });

        modelBuilder.Entity<HPost>(entity =>
        {
            entity.HasKey(e => e.HPostId).HasName("PK__h_Posts__0BD7DF02A96BFA7B");

            entity.ToTable("h_Posts");

            entity.Property(e => e.HPostId).HasColumnName("h_Post_Id");
            entity.Property(e => e.HAuthorId).HasColumnName("h_Author_Id");
            entity.Property(e => e.HAuthorType)
                .HasMaxLength(50)
                .HasColumnName("h_AuthorType");
            entity.Property(e => e.HContent)
                .HasMaxLength(500)
                .HasColumnName("h_Content");
            entity.Property(e => e.HCreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HDeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_DeletedAt");
            entity.Property(e => e.HIsLocked).HasColumnName("h_IsLocked");
            entity.Property(e => e.HIsPinned).HasColumnName("h_IsPinned");
            entity.Property(e => e.HLastreplyTime)
                .HasColumnType("datetime")
                .HasColumnName("h_LastreplyTime");
            entity.Property(e => e.HStatus)
                .HasMaxLength(50)
                .HasColumnName("h_Status");
            entity.Property(e => e.HTitle)
                .HasMaxLength(50)
                .HasColumnName("h_Title");
            entity.Property(e => e.HUpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_UpdatedAt");
        });

        modelBuilder.Entity<HPostMonitoring>(entity =>
        {
            entity.HasKey(e => e.HAbnormalId).HasName("PK__h_Post_M__1FD44915146F5833");

            entity.ToTable("h_Post_Monitoring");

            entity.Property(e => e.HAbnormalId).HasColumnName("h_Abnormal_Id");
            entity.Property(e => e.HAdminId).HasColumnName("h_Admin_Id");
            entity.Property(e => e.HAuthorId).HasColumnName("h_Author_Id");
            entity.Property(e => e.HAuthorType)
                .HasMaxLength(50)
                .HasColumnName("h_AuthorType");
            entity.Property(e => e.HDetectedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_DetectedAt");
            entity.Property(e => e.HPostId).HasColumnName("h_Post_Id");
            entity.Property(e => e.HStatus)
                .HasMaxLength(50)
                .HasColumnName("h_Status");

            entity.HasOne(d => d.HAdmin).WithMany(p => p.HPostMonitorings)
                .HasForeignKey(d => d.HAdminId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_h_PostMonitoring_h_Admin");

            entity.HasOne(d => d.HPost).WithMany(p => p.HPostMonitorings)
                .HasForeignKey(d => d.HPostId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_h_PostMonitoring_h_Posts");
        });

        modelBuilder.Entity<HProperty>(entity =>
        {
            entity.HasKey(e => e.HPropertyId).HasName("PK_h_property 房屋資料表");

            entity.ToTable("h_Property");

            entity.Property(e => e.HPropertyId).HasColumnName("h_Property_Id");
            entity.Property(e => e.HAddress)
                .HasMaxLength(50)
                .HasColumnName("h_Address");
            entity.Property(e => e.HArea).HasColumnName("h_Area");
            entity.Property(e => e.HAvailabilityStatus)
                .HasMaxLength(50)
                .HasColumnName("h_Availability_Status");
            entity.Property(e => e.HBathroomCount).HasColumnName("h_Bathroom_Count");
            entity.Property(e => e.HBuildingType)
                .HasMaxLength(50)
                .HasColumnName("h_Building_Type");
            entity.Property(e => e.HCity)
                .HasMaxLength(50)
                .HasColumnName("h_City");
            entity.Property(e => e.HDescription)
                .HasMaxLength(50)
                .HasColumnName("h_Description");
            entity.Property(e => e.HDistrict)
                .HasMaxLength(50)
                .HasColumnName("h_District");
            entity.Property(e => e.HFloor).HasColumnName("h_Floor");
            entity.Property(e => e.HIsDelete).HasColumnName("h_IsDelete");
            entity.Property(e => e.HIsShared)
                .HasDefaultValue(false)
                .HasColumnName("h_IsShared");
            entity.Property(e => e.HIsVip)
                .HasDefaultValue(false)
                .HasColumnName("h_IsVIP");
            entity.Property(e => e.HLandlordId).HasColumnName("h_Landlord_Id");
            entity.Property(e => e.HLastUpdated)
                .HasColumnType("datetime")
                .HasColumnName("h_Last_Updated");
            entity.Property(e => e.HLatitude)
                .HasMaxLength(50)
                .HasColumnName("h_Latitude");
            entity.Property(e => e.HLongitude)
                .HasMaxLength(50)
                .HasColumnName("h_Longitude");
            entity.Property(e => e.HPropertyTitle)
                .HasMaxLength(50)
                .HasColumnName("h_Property_Title");
            entity.Property(e => e.HPropertyType)
                .HasMaxLength(50)
                .HasColumnName("h_Property_Type");
            entity.Property(e => e.HPublishedDate)
                .HasColumnType("datetime")
                .HasColumnName("h_Published_Date");
            entity.Property(e => e.HRentPrice).HasColumnName("h_Rent_Price");
            entity.Property(e => e.HRoomCount).HasColumnName("h_Room_Count");
            entity.Property(e => e.HScore)
                .HasMaxLength(50)
                .HasColumnName("h_Score");
            entity.Property(e => e.HStatus)
                .HasMaxLength(50)
                .HasColumnName("h_Status");
            entity.Property(e => e.HTotalFloors).HasColumnName("h_Total_Floors");
            entity.Property(e => e.HZipcode)
                .HasMaxLength(50)
                .HasColumnName("h_Zipcode");

            entity.HasOne(d => d.HLandlord).WithMany(p => p.HProperties)
                .HasForeignKey(d => d.HLandlordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_h_Property_h_Landlord");
        });

        modelBuilder.Entity<HPropertyAudit>(entity =>
        {
            entity.HasKey(e => e.HAuditId).HasName("PK_h_property_audit 物件審核資料表");

            entity.ToTable("h_Property_Audit");

            entity.Property(e => e.HAuditId).HasColumnName("h_Audit_Id");
            entity.Property(e => e.HAuditDate)
                .HasColumnType("datetime")
                .HasColumnName("h_AuditDate");
            entity.Property(e => e.HAuditNotes)
                .HasMaxLength(50)
                .HasColumnName("h_AuditNotes");
            entity.Property(e => e.HAuditStatus)
                .HasMaxLength(50)
                .HasColumnName("h_AuditStatus");
            entity.Property(e => e.HAuditorId).HasColumnName("h_Auditor_Id");
            entity.Property(e => e.HIsDelete).HasColumnName("h_IsDelete");
            entity.Property(e => e.HLandlordId).HasColumnName("h_Landlord_Id");
            entity.Property(e => e.HPropertyId).HasColumnName("h_Property_Id");

            entity.HasOne(d => d.HLandlord).WithMany(p => p.HPropertyAudits)
                .HasForeignKey(d => d.HLandlordId)
                .HasConstraintName("FK_h_Property_Audit_h_Landlord");

            entity.HasOne(d => d.HProperty).WithMany(p => p.HPropertyAudits)
                .HasForeignKey(d => d.HPropertyId)
                .HasConstraintName("FK_h_Property_Audit_h_Property");
        });

        modelBuilder.Entity<HPropertyFeature>(entity =>
        {
            entity.HasKey(e => e.HFeaturePropertyId).HasName("PK_h_PropertyFeatures 房源特色");

            entity.ToTable("h_Property_Features");

            entity.Property(e => e.HFeaturePropertyId).HasColumnName("h_FeatureProperty_ID");
            entity.Property(e => e.HAirConditioning).HasColumnName("h_AirConditioning");
            entity.Property(e => e.HAllowsAnimals).HasColumnName("h_AllowsAnimals");
            entity.Property(e => e.HAllowsCats).HasColumnName("h_AllowsCats");
            entity.Property(e => e.HAllowsCooking).HasColumnName("h_AllowsCooking");
            entity.Property(e => e.HAllowsDogs).HasColumnName("h_AllowsDogs");
            entity.Property(e => e.HBalcony).HasColumnName("h_Balcony");
            entity.Property(e => e.HBed).HasColumnName("h_Bed");
            entity.Property(e => e.HCableTv).HasColumnName("h_CableTv");
            entity.Property(e => e.HGasStove).HasColumnName("h_GasStove");
            entity.Property(e => e.HHasFurniture).HasColumnName("h_HasFurniture");
            entity.Property(e => e.HInternet).HasColumnName("h_Internet");
            entity.Property(e => e.HIsDelete).HasColumnName("h_IsDelete");
            entity.Property(e => e.HLandlordId).HasColumnName("h_Landlord_Id");
            entity.Property(e => e.HLandlordShared).HasColumnName("h_LandlordShared");
            entity.Property(e => e.HParking).HasColumnName("h_Parking");
            entity.Property(e => e.HPropertyId).HasColumnName("h_Property_Id");
            entity.Property(e => e.HPublicElectricity).HasColumnName("h_PublicElectricity");
            entity.Property(e => e.HPublicEquipment).HasColumnName("h_PublicEquipment");
            entity.Property(e => e.HPublicWatercharges).HasColumnName("h_PublicWatercharges");
            entity.Property(e => e.HRefrigerator).HasColumnName("h_Refrigerator");
            entity.Property(e => e.HSharedRental).HasColumnName("h_SharedRental");
            entity.Property(e => e.HShortTermRent).HasColumnName("h_ShortTermRent");
            entity.Property(e => e.HSocialHousing).HasColumnName("h_SocialHousing");
            entity.Property(e => e.HTv).HasColumnName("h_Tv");
            entity.Property(e => e.HWashingMachine).HasColumnName("h_WashingMachine");
            entity.Property(e => e.HWaterDispenser).HasColumnName("h_WaterDispenser");
            entity.Property(e => e.HWaterHeater).HasColumnName("h_WaterHeater");

            entity.HasOne(d => d.HLandlord).WithMany(p => p.HPropertyFeatures)
                .HasForeignKey(d => d.HLandlordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_h_Property_Features_h_Landlord");

            entity.HasOne(d => d.HProperty).WithMany(p => p.HPropertyFeatures)
                .HasForeignKey(d => d.HPropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_h_Property_Features_h_Property");
        });

        modelBuilder.Entity<HPropertyImage>(entity =>
        {
            entity.HasKey(e => e.HImageId).HasName("PK_h_property_images 房屋圖片資料表");

            entity.ToTable("h_Property_images");

            entity.Property(e => e.HImageId).HasColumnName("h_Image_Id");
            entity.Property(e => e.HCaption)
                .HasMaxLength(100)
                .HasColumnName("h_Caption");
            entity.Property(e => e.HImageUrl).HasColumnName("h_ImageUrl");
            entity.Property(e => e.HIsDelete).HasColumnName("h_IsDelete");
            entity.Property(e => e.HLandlordId).HasColumnName("h_Landlord_Id");
            entity.Property(e => e.HLastUpDated)
                .HasColumnType("datetime")
                .HasColumnName("h_LastUpDated");
            entity.Property(e => e.HPropertyId).HasColumnName("h_Property_Id");
            entity.Property(e => e.HUploadedDate)
                .HasColumnType("datetime")
                .HasColumnName("h_UploadedDate");

            entity.HasOne(d => d.HLandlord).WithMany(p => p.HPropertyImages)
                .HasForeignKey(d => d.HLandlordId)
                .HasConstraintName("FK_h_Property_images_h_Landlord");

            entity.HasOne(d => d.HProperty).WithMany(p => p.HPropertyImages)
                .HasForeignKey(d => d.HPropertyId)
                .HasConstraintName("FK_h_Property_images_h_Property");
        });

        modelBuilder.Entity<HReaction>(entity =>
        {
            entity.HasKey(e => e.HReactionId).HasName("PK__h_Reacti__BF41BB91F97B4786");

            entity.ToTable("h_Reactions");

            entity.Property(e => e.HReactionId).HasColumnName("h_Reaction_Id");
            entity.Property(e => e.HAuthorType)
                .HasMaxLength(50)
                .HasColumnName("h_AuthorType");
            entity.Property(e => e.HContentType)
                .HasMaxLength(50)
                .HasColumnName("h_ContentType");
            entity.Property(e => e.HCreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HReactionType)
                .HasMaxLength(50)
                .HasColumnName("h_ReactionType");
            entity.Property(e => e.HTargetId).HasColumnName("h_Target_Id");
            entity.Property(e => e.HTargetType)
                .HasMaxLength(50)
                .HasColumnName("h_TargetType");
            entity.Property(e => e.HTenantId).HasColumnName("h_Tenant_Id");

            entity.HasOne(d => d.HTarget).WithMany(p => p.HReactions)
                .HasForeignKey(d => d.HTargetId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_h_Reaction_Target");

            entity.HasOne(d => d.HTenant).WithMany(p => p.HReactions)
                .HasForeignKey(d => d.HTenantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_h_Reaction_Tenant");
        });

        modelBuilder.Entity<HReply>(entity =>
        {
            entity.HasKey(e => e.HReplyId).HasName("PK__h_Replie__C779AD61598B755C");

            entity.ToTable("h_Replies");

            entity.Property(e => e.HReplyId).HasColumnName("h_Reply_Id");
            entity.Property(e => e.HAuthorId).HasColumnName("h_Author_Id");
            entity.Property(e => e.HAuthorType)
                .HasMaxLength(50)
                .HasColumnName("h_AuthorType");
            entity.Property(e => e.HContent)
                .HasMaxLength(500)
                .HasColumnName("h_Content");
            entity.Property(e => e.HCreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HDeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_DeletedAt");
            entity.Property(e => e.HPostId).HasColumnName("h_Post_Id");
            entity.Property(e => e.HStatus)
                .HasMaxLength(50)
                .HasColumnName("h_Status");
        });

        modelBuilder.Entity<HReport>(entity =>
        {
            entity.HasKey(e => e.HReportId).HasName("PK__h_Report__53849708AED1476C");

            entity.ToTable("h_Reports");

            entity.Property(e => e.HReportId).HasColumnName("h_ReportId");
            entity.Property(e => e.HAdminId).HasColumnName("h_Admin_Id");
            entity.Property(e => e.HAdminNote)
                .HasMaxLength(255)
                .HasColumnName("h_AdminNote");
            entity.Property(e => e.HAuthorId).HasColumnName("h_Author_Id");
            entity.Property(e => e.HAuthorType)
                .HasMaxLength(50)
                .HasColumnName("h_AuthorType");
            entity.Property(e => e.HCreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HMessageId).HasColumnName("h_MessageId");
            entity.Property(e => e.HReason)
                .HasMaxLength(255)
                .HasColumnName("h_Reason");
            entity.Property(e => e.HRelatedChatId).HasColumnName("h_RelatedChatId");
            entity.Property(e => e.HReportType)
                .HasMaxLength(50)
                .HasColumnName("h_Report_Type");
            entity.Property(e => e.HReportedUserId).HasColumnName("h_ReportedUser_Id");
            entity.Property(e => e.HReviewedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_ReviewedAt");
            entity.Property(e => e.HStatus)
                .HasMaxLength(50)
                .HasColumnName("h_Status");

            entity.HasOne(d => d.HAdmin).WithMany(p => p.HReports)
                .HasForeignKey(d => d.HAdminId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_h_Reports_h_Admin");

            entity.HasOne(d => d.HMessage).WithMany(p => p.HReports)
                .HasForeignKey(d => d.HMessageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_h_Reports_h_Messages");

            entity.HasOne(d => d.HReportedUser).WithMany(p => p.HReports)
                .HasForeignKey(d => d.HReportedUserId)
                .HasConstraintName("FK_h_Report_ReportedUser");
        });

        modelBuilder.Entity<HReportForum>(entity =>
        {
            entity.HasKey(e => e.HReportForumId).HasName("PK__h_Report__DB82E3D12C07B2BF");

            entity.ToTable("h_ReportForum");

            entity.Property(e => e.HReportForumId).HasColumnName("h_ReportForum_Id");
            entity.Property(e => e.HAdminId).HasColumnName("h_Admin_Id");
            entity.Property(e => e.HCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HHandledAt)
                .HasColumnType("datetime")
                .HasColumnName("h_HandledAt");
            entity.Property(e => e.HReason)
                .HasMaxLength(500)
                .HasColumnName("h_Reason");
            entity.Property(e => e.HReporterId).HasColumnName("h_Reporter_Id");
            entity.Property(e => e.HReporterType)
                .HasMaxLength(50)
                .HasColumnName("h_ReporterType");
            entity.Property(e => e.HStatus)
                .HasMaxLength(50)
                .HasDefaultValue("待審核")
                .HasColumnName("h_Status");
            entity.Property(e => e.HTargetId).HasColumnName("h_TargetId");
            entity.Property(e => e.HTargetType)
                .HasMaxLength(50)
                .HasColumnName("h_TargetType");

            entity.HasOne(d => d.HAdmin).WithMany(p => p.HReportForums)
                .HasForeignKey(d => d.HAdminId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_h_ReportForum_h_Admin");
        });

        modelBuilder.Entity<HRevenueReport>(entity =>
        {
            entity.HasKey(e => e.HReportId).HasName("PK__h_Revenu__4E24F435929EF0EF");

            entity.ToTable("h_Revenue_Report");

            entity.Property(e => e.HReportId).HasColumnName("h_Report_Id");
            entity.Property(e => e.HDailyIncome)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("h_DailyIncome");
            entity.Property(e => e.HGeneratedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_GeneratedAt");
            entity.Property(e => e.HGrowthRate)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("h_GrowthRate");
            entity.Property(e => e.HMonthlyIncome)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("h_MonthlyIncome");
            entity.Property(e => e.HPaymentMethods).HasColumnName("h_PaymentMethods");
            entity.Property(e => e.HReportDate)
                .HasColumnType("datetime")
                .HasColumnName("h_ReportDate");
            entity.Property(e => e.HReportPeriod)
                .HasColumnType("datetime")
                .HasColumnName("h_ReportPeriod");
            entity.Property(e => e.HTotalIncome)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("h_TotalIncome");
            entity.Property(e => e.HTotalTransactions).HasColumnName("h_TotalTransactions");
        });

        modelBuilder.Entity<HSso>(entity =>
        {
            entity.HasKey(e => e.HSsoId).HasName("PK__h_SSO__078BAF04ADB3B06C");

            entity.ToTable("h_SSO");

            entity.HasIndex(e => e.HUserEmail, "UQ__h_SSO__D5BFBB2AC3E6485E").IsUnique();

            entity.Property(e => e.HSsoId).HasColumnName("h_SSO_Id");
            entity.Property(e => e.HAud)
                .HasMaxLength(255)
                .HasColumnName("h_aud");
            entity.Property(e => e.HEmailverified).HasColumnName("h_emailverified");
            entity.Property(e => e.HExp)
                .HasColumnType("datetime")
                .HasColumnName("h_exp");
            entity.Property(e => e.HIat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("h_iat");
            entity.Property(e => e.HSub)
                .HasMaxLength(255)
                .HasColumnName("h_sub");
            entity.Property(e => e.HTenantId).HasColumnName("h_Tenant_Id");
            entity.Property(e => e.HUserEmail)
                .HasMaxLength(100)
                .HasColumnName("h_userEmail");

            entity.HasOne(d => d.HTenant).WithMany(p => p.HSsos)
                .HasForeignKey(d => d.HTenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_h_SSO_Tenant");
        });

        modelBuilder.Entity<HTenant>(entity =>
        {
            entity.HasKey(e => e.HTenantId).HasName("PK__h_Tenant__3A0E244A59911E86");

            entity.ToTable("h_Tenant");

            entity.HasIndex(e => e.HEmail, "UQ__h_Tenant__C26FFCAEC1BC3BC8").IsUnique();

            entity.Property(e => e.HTenantId).HasColumnName("h_Tenant_Id");
            entity.Property(e => e.HAddress).HasColumnName("h_Address");
            entity.Property(e => e.HBirthday)
                .HasColumnType("datetime")
                .HasColumnName("h_Birthday");
            entity.Property(e => e.HCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HEmail)
                .HasMaxLength(255)
                .HasColumnName("h_Email");
            entity.Property(e => e.HGender).HasColumnName("h_Gender");
            entity.Property(e => e.HImages).HasColumnName("h_Images");
            entity.Property(e => e.HIsDeleted).HasColumnName("h_IsDeleted");
            entity.Property(e => e.HIsLandlord).HasColumnName("h_IsLandlord");
            entity.Property(e => e.HIsTenant).HasColumnName("h_IsTenant");
            entity.Property(e => e.HLastLoginAt)
                .HasColumnType("datetime")
                .HasColumnName("h_LastLoginAt");
            entity.Property(e => e.HLastLoginIp)
                .HasMaxLength(50)
                .HasColumnName("h_LastLoginIP");
            entity.Property(e => e.HLockoutEnd)
                .HasColumnType("datetime")
                .HasColumnName("h_LockoutEnd");
            entity.Property(e => e.HLoginFailCount).HasColumnName("h_LoginFailCount");
            entity.Property(e => e.HPassword)
                .HasMaxLength(255)
                .HasColumnName("h_Password");
            entity.Property(e => e.HPhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("h_PhoneNumber");
            entity.Property(e => e.HSalt)
                .HasMaxLength(100)
                .HasColumnName("h_Salt");
            entity.Property(e => e.HStatus)
                .HasMaxLength(50)
                .HasDefaultValue("未驗證")
                .HasColumnName("h_Status");
            entity.Property(e => e.HUpdateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("h_UpdateAt");
            entity.Property(e => e.HUserName)
                .HasMaxLength(100)
                .HasColumnName("h_UserName");
        });

        modelBuilder.Entity<HTransaction>(entity =>
        {
            entity.HasKey(e => e.HPaymentId).HasName("PK__h_Transa__2FCF77938A95683E");

            entity.ToTable("h_Transactions");

            entity.Property(e => e.HPaymentId).HasColumnName("h_Payment_Id");
            entity.Property(e => e.HAdId).HasColumnName("h_Ad_Id");
            entity.Property(e => e.HAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("h_Amount");
            entity.Property(e => e.HCreateTime)
                .HasColumnType("datetime")
                .HasColumnName("h_Create_Time");
            entity.Property(e => e.HIsSimulated).HasColumnName("h_Is_Simulated");
            entity.Property(e => e.HItemName)
                .HasMaxLength(100)
                .HasColumnName("h_Item_Name");
            entity.Property(e => e.HMerchantTradeNo)
                .HasMaxLength(50)
                .HasColumnName("h_Merchant_Trade_No");
            entity.Property(e => e.HPaymentDate)
                .HasColumnType("datetime")
                .HasColumnName("h_Payment_Date");
            entity.Property(e => e.HPaymentType)
                .HasMaxLength(50)
                .HasColumnName("h_Payment_Type");
            entity.Property(e => e.HPropertyId).HasColumnName("h_Property_Id");
            entity.Property(e => e.HRawJson).HasColumnName("h_Raw_Json");
            entity.Property(e => e.HRegion)
                .HasMaxLength(50)
                .HasColumnName("h_Region");
            entity.Property(e => e.HRtnMsg)
                .HasMaxLength(200)
                .HasColumnName("h_Rtn_Msg");
            entity.Property(e => e.HTradeNo)
                .HasMaxLength(50)
                .HasColumnName("h_Trade_No");
            entity.Property(e => e.HTradeStatus)
                .HasMaxLength(20)
                .HasColumnName("h_Trade_Status");
            entity.Property(e => e.HUpdateTime)
                .HasColumnType("datetime")
                .HasColumnName("h_Update_Time");

            entity.HasOne(d => d.HAd).WithMany(p => p.HTransactions)
                .HasForeignKey(d => d.HAdId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_h_Transactions_h_Ad");

            entity.HasOne(d => d.HProperty).WithMany(p => p.HTransactions)
                .HasForeignKey(d => d.HPropertyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_h_Transactions_h_Property");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
