using ResearchScrapper.Api.Service;

namespace ResearchScrapper.Api.MiddleWare{
    public class JwtMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IConfiguration configuration;
        private readonly ILogger<JwtMiddleware> _logger;

        public JwtMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<JwtMiddleware> logger)
        {
            this.next = next;
            this.configuration = configuration;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, JwtService jwtService)
        {
            try{
                var token = context.Request.Cookies["jwt"];
  
                if (token is not null) AttachUserToContext(context, token, jwtService);

                await next(context);
            }
            catch(Exception ex){
                Console.Write(ex);
            }
            
        }

        private static void  AttachUserToContext(HttpContext context, string token, JwtService jwtService)
        {
            try
            {
                jwtService.ValidateToken(token);
                var userInfo = jwtService.ParseToken(token);

                context.Items["User"] = userInfo;

            }
            catch (Exception e){
                Console.Write(e);
            }
        }
    }
}