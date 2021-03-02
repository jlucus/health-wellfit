using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using WellFitPlus.Database.Contexts;
using WellFitPlus.Database.Entities.Identity;
using WellFitPlus.WebAPI.Utility;
using System.Threading.Tasks;

namespace WellFitPlus.WebAPI {

    public class EmailService : IIdentityMessageService {
        public Task SendAsync(IdentityMessage message) {
            // Plug in your email service here to send an email.
            (new EmailUtility()).AsyncSendMessage(message.Destination, message.Subject, message.Body);

            return Task.FromResult(0);
        }
    }

    public class ApplicationUserManager : UserManager<ApplicationUser> {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store) {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<WellFitAuthContext>()));

            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            manager.EmailService = new EmailService();

            var dataProtectionProvider = options.DataProtectionProvider;

            if (dataProtectionProvider != null) {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("Well Fit Plus Identity"));
            }

            return manager;
        }
    }
}
