﻿namespace DreamFactory.Tests.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DreamFactory.Api;
    using DreamFactory.Api.Implementation;
    using DreamFactory.Http;
    using DreamFactory.Model.Database;
    using DreamFactory.Model.System.App;
    using DreamFactory.Model.System.AppGroup;
    using DreamFactory.Model.System.Config;
    using DreamFactory.Model.System.Cors;
    using DreamFactory.Model.System.Email;
    using DreamFactory.Model.System.Environment;
    using DreamFactory.Model.System.Event;
    using DreamFactory.Model.System.Lookup;
    using DreamFactory.Model.System.Role;
    using DreamFactory.Model.System.Script;
    using DreamFactory.Model.System.Service;
    using DreamFactory.Model.System.User;
    using DreamFactory.Model.User;
    using DreamFactory.Rest;
    using DreamFactory.Serialization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;

    [TestClass]
    public class SystemApiTests
    {
        private static ISystemApi CreateSystemApi(string suffix = null)
        {
            HttpHeaders headers;
            return CreateSystemApi(out headers, suffix);
        }

        private static ISystemApi CreateSystemApi(out HttpHeaders headers, string suffix = null)
        {
            IHttpFacade httpFacade = new TestDataHttpFacade(suffix);
            HttpAddress address = new HttpAddress("http://base_address", RestApiVersion.V1);
            headers = new HttpHeaders();
            return new SystemApi(address, httpFacade, new JsonContentSerializer(), headers);
        }

        #region --- Session ---

        [TestMethod]
        public void ShouldLoginAdminAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            Session session = systemApi.LoginAdminAsync("dream@factory.com", "dreamfactory").Result;

            // Assert
            session.Name.ShouldBe("SuperAdmin");
            session.SessionId.ShouldNotBeNullOrEmpty();

            Should.Throw<ArgumentNullException>(() => systemApi.LoginAdminAsync(null, "dreamfactory"));
            Should.Throw<ArgumentNullException>(() => systemApi.LoginAdminAsync("dream@factory.com", null));
            Should.Throw<ArgumentOutOfRangeException>(() => systemApi.LoginAdminAsync("dream@factory.com", "dreamfactory", -9999));
        }

        [TestMethod]
        public void ShouldChangeAdminPasswordAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            bool result = systemApi.ChangeAdminPasswordAsync("oldPassword", "newPassword").Result;

            // Assert
            result.ShouldBe(true);

            Should.Throw<ArgumentNullException>(() => systemApi.ChangeAdminPasswordAsync(null, "newPassword"));
            Should.Throw<ArgumentNullException>(() => systemApi.ChangeAdminPasswordAsync("oldPassword", null));
        }

        [TestMethod]
        public void ShouldGetAdminSessionAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            systemApi.LoginAdminAsync("dream@factory.com", "dreamfactory").Wait();

            // Act
            Session session = systemApi.GetAdminSessionAsync().Result;

            // Assert
            session.SessionId.ShouldNotBeNullOrEmpty();
        }

        [TestMethod]
        public void LoginAdminShouldChangeBaseHeaders()
        {
            // Arrange
            HttpHeaders headers;
            ISystemApi systemApi = CreateSystemApi(out headers);

            // Act
            systemApi.LoginAdminAsync("dream@factory.com", "dreamfactory").Wait();

            // Assert
            Dictionary<string, object> dictionary = headers.Build();
            dictionary.ContainsKey(HttpHeaders.DreamFactorySessionTokenHeader).ShouldBe(true);
        }

        [TestMethod]
        public void ShouldLogoutAdminAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            bool logout = systemApi.LogoutAdminAsync().Result;

            // Assert
            logout.ShouldBe(true);
        }

        [TestMethod]
        public void LogoutAdminShouldRemoveSessionToken()
        {
            // Arrange
            HttpHeaders headers;
            ISystemApi systemApi = CreateSystemApi(out headers);
            systemApi.LoginAdminAsync("dream@factory.com", "dreamfactory").Wait();

            // Act
            systemApi.LogoutAdminAsync().Wait();

            // Assert
            headers.Build().ContainsKey(HttpHeaders.DreamFactorySessionTokenHeader).ShouldBe(false);
        }

        #endregion

        #region --- EmailTemplate ---

        [TestMethod]
        public void ShouldGetEmailTemplatesAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            List<EmailTemplateResponse> emailTemplates = systemApi.GetEmailTemplatesAsync(new SqlQuery()).Result.ToList();

            // Assert
            emailTemplates.Count.ShouldBe(3);
            emailTemplates.First().Name.ShouldBe("User Invite Default");
        }

        [TestMethod]
        public void ShouldCreateEmailTemplatesAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            EmailTemplateRequest[] templates = CreateEmailTemplates();
            foreach (EmailTemplateRequest template in templates)
            {
                template.Id = null;
            }

            // Act
            List<EmailTemplateResponse> emailTemplates = systemApi.CreateEmailTemplatesAsync(new SqlQuery(), templates).Result.ToList();

            // Assert
            emailTemplates.Count.ShouldBe(3);
            emailTemplates.First().Name.ShouldBe("User Invite Default");

            Should.Throw<ArgumentException>(() => systemApi.CreateEmailTemplatesAsync(new SqlQuery(), null));
        }

        [TestMethod]
        public void ShouldUpdateEmailTemplatesAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            EmailTemplateRequest[] templates = CreateEmailTemplates();

            // Act
            List<EmailTemplateResponse> emailTemplates = systemApi.UpdateEmailTemplatesAsync(new SqlQuery(), templates).Result.ToList();

            // Assert
            emailTemplates.Count.ShouldBe(3);
            emailTemplates.First().Name.ShouldBe("User Invite Default");

            Should.Throw<ArgumentException>(() => systemApi.UpdateEmailTemplatesAsync(new SqlQuery(), null));
        }

        [TestMethod]
        public void ShouldDeleteEmailTemplatesAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            EmailTemplateRequest[] templates = CreateEmailTemplates();
            int[] ids = templates.Where(x => x.Id != null).Select(x => x.Id.Value).ToArray();

            // Act
            List<EmailTemplateResponse> emailTemplates = systemApi.DeleteEmailTemplatesAsync(new SqlQuery(), ids).Result.ToList();

            // Assert
            emailTemplates.Count.ShouldBe(3);
            emailTemplates.First().Name.ShouldBe("User Invite Default");

            Should.Throw<ArgumentException>(() => systemApi.DeleteEmailTemplatesAsync(new SqlQuery(), null));
        }

        private EmailTemplateRequest[] CreateEmailTemplates()
        {
            return new []
            {
                new EmailTemplateRequest
                {
                    Id = 1,
                    Name = "User Invite Default",
                },
                new EmailTemplateRequest
                {
                    Id = 2,
                    Name = "User Registration Default"
                },
                new EmailTemplateRequest
                {
                    Id = 3,
                    Name = "Password reset Default"
                }
            };
        }

        #endregion

        #region --- EventScript ---

        [TestMethod]
        public void ShouldGetEventScriptsAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            List<string> events = systemApi.GetEventsAsync().Result.ToList();

            // Assert
            events.Count.ShouldBe(5);
            events.First().ShouldBe("system.get.pre_process");
        }

        [TestMethod]
        public void ShouldGetScriptTypes()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            List<ScriptTypeResponse> scriptTypes = systemApi.GetScriptTypesAsync(new SqlQuery()).Result.ToList();

            // Assert
            scriptTypes.Count.ShouldBe(1);
            scriptTypes.Select(x => x.Name).First().ShouldBe("v8js");
        }

        [TestMethod]
        public void ShouldCreateEventScriptAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            EventScriptRequest eventScript = CreateEventScript();

            // Act
            EventScriptResponse response = systemApi.CreateEventScriptAsync("system.get.pre_process", new SqlQuery(), eventScript).Result;

            // Assert
            response.Name.ShouldBe("my_custom_script");
            response.Type.ShouldBe("v8js");

            Should.Throw<ArgumentNullException>(() => systemApi.CreateEventScriptAsync(null, new SqlQuery(), eventScript));
            Should.Throw<ArgumentNullException>(() => systemApi.CreateEventScriptAsync("system.get.pre_process", new SqlQuery(), null));
        }

        [TestMethod]
        public void ShouldDeleteEventScriptAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            EventScriptResponse response = systemApi.DeleteEventScriptAsync("system.get.pre_process", new SqlQuery()).Result;

            // Assert
            response.Name.ShouldBe("my_custom_script");
            response.Type.ShouldBe("v8js");

            Should.Throw<ArgumentNullException>(() => systemApi.DeleteEventScriptAsync(null, new SqlQuery()));
        }

        [TestMethod]
        public void ShouldGetEventsAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            EventScriptResponse response = systemApi.GetEventScriptAsync("system.get.pre_process", new SqlQuery()).Result;

            // Assert
            response.Name.ShouldBe("my_custom_script");
            response.Type.ShouldBe("v8js");

            Should.Throw<ArgumentNullException>(() => systemApi.GetEventScriptAsync(null, new SqlQuery()));
        }

        private static EventScriptRequest CreateEventScript()
        {
            return new EventScriptRequest
            {
                Name = "my_custom_script",
                Type = "v8js",
                IsActive = true,
                AffectsProcess = true,
                Content = "text",
                Config = "text"
            };
        }

        #endregion

        #region --- App ---

        [TestMethod]
        public void ShouldGetAppsAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            List<AppResponse> apps = systemApi.GetAppsAsync(new SqlQuery()).Result.ToList();

            // Assert
            apps.Count.ShouldBe(4);
            apps.First().Name.ShouldBe("admin");
        }

        [TestMethod]
        public void ShouldCreateAppAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            AppRequest app = CreateApp();
            app.Id = null;

            // Act
            AppResponse created = systemApi.CreateAppsAsync(new SqlQuery(), app).Result.First();

            // Assert
            created.Id.ShouldBe(1);

            Should.Throw<ArgumentException>(() => systemApi.CreateAppsAsync(new SqlQuery()));
        }

        [TestMethod]
        public void ShouldUpdateAppAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            AppRequest app = CreateApp();

            // Act & Assert
            systemApi.UpdateAppsAsync(new SqlQuery(), app).Wait();
        }

        [TestMethod]
        public void ShouldDeleteAppsAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act & Assert
            systemApi.DeleteAppsAsync(new SqlQuery(), 1, 2, 3);

            Should.Throw<ArgumentException>(() => systemApi.DeleteAppsAsync(new SqlQuery()));
        }

        private static AppRequest CreateApp()
        {
            return new AppRequest
            {
                Id = 1,
                Name = "admin",
                Description = "An application for administering this instance.",
                IsActive = true,
                RequiresFullscreen = false,
                AllowFullscreenToggle = true,
                ToggleLocation = "top",
                RoleId = 2
            };
        }

        #endregion

        #region --- Environment ---

        [TestMethod]
        public void ShouldGetEnvironmentAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            EnvironmentResponse environment = systemApi.GetEnvironmentAsync().Result;

            // Assert
            environment.Platform.VersionCurrent.ShouldBe("2.0");
        }

        #endregion

        #region --- Config ---

        [TestMethod]
        public void ShouldGetConfigAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            ConfigResponse config = systemApi.GetConfigAsync().Result;

            // Assert
            config.EditableProfileFields.ShouldBe("name");
        }

        [TestMethod]
        public void ShouldSetConfigAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            ConfigRequest config = CreateConfig();

            // Act
            ConfigResponse response = systemApi.SetConfigAsync(config).Result;

            // Assert
            response.EditableProfileFields.ShouldBe("name");

            Should.Throw<ArgumentNullException>(() => systemApi.SetConfigAsync(null));
        }
        private ConfigRequest CreateConfig()
        {
            return new ConfigRequest
            {
                EditableProfileFields = "name",
                RestrictedVerbs = new List<string> { "patch" },
                TimestampFormat = ""
            };
        }

        #endregion

        #region --- Constant ---

        [TestMethod]
        public void ShouldGetConstantsAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            List<string> types = systemApi.GetConstantsAsync().Result.ToList();

            // Assert
            types.Count.ShouldBe(19);
            types.ShouldContain("content_types");
        }

        [TestMethod]
        public void ShouldGetConstantAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            Dictionary<string, string> constant = systemApi.GetConstantAsync("content_types").Result;

            // Assert
            constant.Count.ShouldBe(14);
            constant.Keys.ShouldContain("HTML");
        }

        #endregion

        #region --- AppGroup ---

        [TestMethod]
        public void ShouldGetAppGroupAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            List<AppGroupResponse> appGroups = systemApi.GetAppGroupsAsync(new SqlQuery()).Result.ToList();

            // Assert
            appGroups.Count.ShouldBe(1);
            appGroups.First().Name.ShouldBe("my_app_group");
        }

        [TestMethod]
        public void ShouldCreateAppGroupAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            AppGroupRequest appGroup = CreateAppGroup();
            appGroup.Id = null;

            // Act
            AppGroupResponse created = systemApi.CreateAppGroupsAsync(new SqlQuery(), appGroup).Result.First();

            // Assert
            created.Id.ShouldBe(1);

            Should.Throw<ArgumentException>(() => systemApi.CreateAppGroupsAsync(new SqlQuery()));
        }

        [TestMethod]
        public void ShouldUpdateAppGroupAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            AppGroupRequest appGroup = CreateAppGroup();

            // Act & Assert
            systemApi.UpdateAppGroupsAsync(new SqlQuery(), appGroup).Wait();
        }

        [TestMethod]
        public void ShouldDeleteAppGroupsAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act & Assert
            systemApi.DeleteAppGroupsAsync(new SqlQuery(), 1);

            Should.Throw<ArgumentException>(() => systemApi.DeleteAppGroupsAsync(new SqlQuery()));
        }

        private static AppGroupRequest CreateAppGroup()
        {
            return new AppGroupRequest
            {
                Id = 1,
                Name = "my_app_group",
                Description = "Contains my groups."
            };
        }

        #endregion

        #region --- Service ---

        [TestMethod]
        public void ShouldGetServicesAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            List<ServiceResponse> services = systemApi.GetServicesAsync(new SqlQuery()).Result.ToList();

            // Assert
            services.Count.ShouldBe(3);
            services.First().Name.ShouldBe("system");
        }

        [TestMethod]
        public void ShouldCreateServiceAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            ServiceRequest service = CreateService();
            service.Id = null;

            // Act
            ServiceResponse created = systemApi.CreateServicesAsync(new SqlQuery(), service).Result.First();

            // Assert
            created.Id.ShouldBe(1);

            Should.Throw<ArgumentException>(() => systemApi.CreateServicesAsync(new SqlQuery()));
        }

        [TestMethod]
        public void ShouldUpdateServiceAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            ServiceRequest service = CreateService();

            // Act & Assert
            systemApi.UpdateServicesAsync(new SqlQuery(), service).Wait();
        }

        [TestMethod]
        public void ShouldDeleteServicesAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act & Assert
            systemApi.DeleteServicesAsync(new SqlQuery(), 1, 2, 3);

            Should.Throw<ArgumentException>(() => systemApi.DeleteServicesAsync(new SqlQuery()));
        }

        private static ServiceRequest CreateService()
        {
            return new ServiceRequest
            {
                Id = 1,
                Name = "system",
                Label = "System Management",
                Description = "Service for managing system resources.",
                IsActive = true,
                Type= "system"
            };
        }

        #endregion

        #region --- Role ---

        [TestMethod]
        public void ShouldGetRolesAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            List<RoleResponse> roles = systemApi.GetRolesAsync(new SqlQuery()).Result.ToList();

            // Assert
            roles.Count.ShouldBe(2);
            roles.First().Name.ShouldBe("AddressBookUser");
        }

        [TestMethod]
        public void ShouldCreateRoleAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            RoleRequest role = CreateRole();
            role.Id = null;

            // Act
            RoleResponse created = systemApi.CreateRolesAsync(new SqlQuery(), role).Result.First();

            // Assert
            created.Id.ShouldBe(1);

            Should.Throw<ArgumentException>(() => systemApi.CreateRolesAsync(new SqlQuery()));
        }

        [TestMethod]
        public void ShouldUpdateRoleAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            RoleRequest role = CreateRole();

            // Act & Assert
            systemApi.UpdateRolesAsync(new SqlQuery(), role).Wait();
        }

        [TestMethod]
        public void ShouldDeleteRolesAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act & Assert
            systemApi.DeleteRolesAsync(new SqlQuery(), 1);

            Should.Throw<ArgumentException>(() => systemApi.DeleteRolesAsync(new SqlQuery()));
        }

        private static RoleRequest CreateRole()
        {
            return new RoleRequest
            {
                Id = 1,
                Name = "AddressBookUser",
                Description = "This role can access address book.",
                IsActive = true,
            };
        }

        #endregion

        #region --- User ---

        [TestMethod]
        public void ShouldGetUsersAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            List<UserResponse> users = systemApi.GetUsersAsync(new SqlQuery()).Result.ToList();

            // Assert
            users.Count.ShouldBe(2);
            users.First().Name.ShouldBe("demo");
        }

        [TestMethod]
        public void ShouldCreateUserAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            UserRequest user = CreateUser();
            user.Id = null;

            // Act
            UserResponse created = systemApi.CreateUsersAsync(new SqlQuery(), user).Result.First();

            // Assert
            created.Id.ShouldBe(1);

            Should.Throw<ArgumentException>(() => systemApi.CreateUsersAsync(new SqlQuery()));
        }

        [TestMethod]
        public void ShouldUpdateUserAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            UserRequest user = CreateUser();

            // Act & Assert
            systemApi.UpdateUsersAsync(new SqlQuery(), user).Wait();
        }

        [TestMethod]
        public void ShouldDeleteUsersAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act & Assert
            systemApi.DeleteUsersAsync(new SqlQuery(), 1);

            Should.Throw<ArgumentException>(() => systemApi.DeleteUsersAsync(new SqlQuery()));
        }

        private static UserRequest CreateUser()
        {
            return new UserRequest
            {
                Id = 1,
                Name = "dreamUser",
                FirstName = "Dream",
                LastName = "Factory",
                Email = "system@factory.com",
                IsActive = true,
            };
        }

        #endregion

        #region --- CORS ---

        [TestMethod]
        public void ShouldGetCorsAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            List<CorsResponse> results = systemApi.GetCorsAsync(new SqlQuery()).Result.ToList();

            // Assert
            results.Count.ShouldBe(2);
            results.First().Origin.ShouldBe("http://domain.foo");
        }

        [TestMethod]
        public void ShouldCreateCorsAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            CorsRequest[] cors = CreateCorsRecords();
            foreach (CorsRequest record in cors)
            {
                record.Id = null;
            }

            // Act
            List<CorsResponse> results = systemApi.CreateCorsAsync(new SqlQuery(), cors).Result.ToList();

            // Assert
            results.Count.ShouldBe(2);
            results.First().Origin.ShouldBe("http://domain.foo");

            Should.Throw<ArgumentException>(() => systemApi.CreateCorsAsync(new SqlQuery(), null));
        }

        [TestMethod]
        public void ShouldUpdateCorsAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            CorsRequest[] cors = CreateCorsRecords();

            // Act
            List<CorsResponse> results = systemApi.UpdateCorsAsync(new SqlQuery(), cors).Result.ToList();

            // Assert
            results.Count.ShouldBe(2);
            results.First().Origin.ShouldBe("http://domain.foo");

            Should.Throw<ArgumentException>(() => systemApi.UpdateCorsAsync(new SqlQuery(), null));
        }

        [TestMethod]
        public void ShouldDeleteCorsAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            CorsRequest[] cors = CreateCorsRecords();
            int[] ids = cors.Where(x => x.Id != null).Select(x => x.Id.Value).ToArray();

            // Act
            List<CorsResponse> results = systemApi.DeleteCorsAsync(new SqlQuery(), ids).Result.ToList();

            // Assert
            results.Count.ShouldBe(2);
            results.First().Origin.ShouldBe("http://domain.foo");

            Should.Throw<ArgumentException>(() => systemApi.DeleteCorsAsync(new SqlQuery(), null));
        }

        private CorsRequest[] CreateCorsRecords()
        {
            return new[]
            {
                new CorsRequest
                {
                    Id = 1,
                    Path = "http://domain.foo",
                    Origin = "http://domain.foo",
                    Enabled = true
                },
                new CorsRequest
                {
                    Id = 2,
                    Path = "http://domain.bar",
                    Origin = "http://domain.bar",
                    Enabled = true
                }
            };
        }

        #endregion

        #region --- Lookup ---

        [TestMethod]
        public void ShouldGetLookupsAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act
            List<LookupResponse> lookups = systemApi.GetLookupsAsync(new SqlQuery()).Result.ToList();

            // Assert
            lookups.Count.ShouldBe(1);
            lookups.First().Value.ShouldBe("text");
        }

        [TestMethod]
        public void ShouldCreateLookupAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            LookupRequest lookup = CreateLookup();
            lookup.Id = null;

            // Act
            LookupResponse created = systemApi.CreateLookupsAsync(new SqlQuery(), lookup).Result.First();

            // Assert
            created.Id.ShouldBe(1);

            Should.Throw<ArgumentException>(() => systemApi.CreateLookupsAsync(new SqlQuery()));
        }

        [TestMethod]
        public void ShouldUpdateLookupAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();
            LookupRequest lookup = CreateLookup();

            // Act & Assert
            systemApi.UpdateLookupsAsync(new SqlQuery(), lookup).Wait();
        }

        [TestMethod]
        public void ShouldDeleteLookupsAsync()
        {
            // Arrange
            ISystemApi systemApi = CreateSystemApi();

            // Act & Assert
            systemApi.DeleteLookupsAsync(new SqlQuery(), 1, 2, 3);

            Should.Throw<ArgumentException>(() => systemApi.DeleteLookupsAsync(new SqlQuery()));
        }

        private static LookupRequest CreateLookup()
        {
            return new LookupRequest
            {
                Id = 1,
                Name = "First",
                Value = "text",
                Private = false,
                Description = "text"
            };
        }

        #endregion

    }
}