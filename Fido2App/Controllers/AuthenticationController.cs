using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Service.Authentication;
using Core.Results.Base;
using Domain.Dto.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Fido2App.Models;
using Fido2App.Models.Authentication;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

namespace Fido2App.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IAuthenticationService _authenticationService;
        private readonly IDataProtector _protector;

        public AuthenticationController(ILogger<AuthenticationController> logger, IAuthenticationService authenticationService, IDataProtectionProvider provider)
        {
            _logger = logger;
            _authenticationService = authenticationService;
            _protector = provider.CreateProtector("passwordless");
        }

        #region public async Task<JsonResult> MakeCredentialOptions([FromBody] MakeCredentialOptionsViewModel makeCredentialOptionsViewModel)

        [HttpPost]
        [Route("/makeCredentialOptions")]
        public async Task<JsonResult> MakeCredentialOptions([FromBody] MakeCredentialOptionsViewModel makeCredentialOptionsViewModel)
        {
            var result = await _authenticationService.MakeCredentialOptions(new MakeCredentialOptionsRequestDto
            {
                Username = makeCredentialOptionsViewModel.Username,
                DisplayName = makeCredentialOptionsViewModel.DisplayName,
                UserVerification = makeCredentialOptionsViewModel.UserVerification,
                AuthenticatorAttachment = makeCredentialOptionsViewModel.AuthenticatorAttachment,
                AttestationType = makeCredentialOptionsViewModel.AttestationType,
                RequireResidentKey = makeCredentialOptionsViewModel.RequireResidentKey
            });

            if (result.Status == StatusTypeEnum.Success)
            {
                // 3. Temporarily store options, session/in-memory cache/redis/db 
                var cookieOptions = new CookieOptions()
                {
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddMinutes(2),
                    HttpOnly = true,
                };
                var content = _protector.Protect(result.Data.ToJson());
                HttpContext?.Response.Cookies.Append("fido2.attestationOptions", content, cookieOptions);

                // 4. return options to client
                return Json(result.Data);
            }
            _logger.Log(LogLevel.Error,result.Message);
            return Json(new {isSuccess = false , message = result.Message});
        }

        #endregion

        #region public async Task<IActionResult> MakeCredential([FromBody] MakeCredentialViewModel makeCredentialViewModel)

        [HttpPost]
        [Route("/makeCredential")]
        public async Task<IActionResult> MakeCredential([FromBody] MakeCredentialViewModel makeCredentialViewModel)
        {
            // 1. get the options we sent the client
            if (string.IsNullOrEmpty(HttpContext?.Request.Cookies["fido2.attestationOptions"]))
                return BadRequest();
            
            var jsonOptions = _protector.Unprotect(HttpContext?.Request.Cookies["fido2.attestationOptions"]!);
            var options = CredentialCreateOptions.FromJson(jsonOptions);

            var result = await _authenticationService.MakeCredential(new MakeCredentialRequestDto
            {
                AuthenticatorAttestationRawResponse = makeCredentialViewModel.AuthenticatorAttestationRawResponse,
                CredentialCreateOptions = options
            });
            
            if (result.Status == StatusTypeEnum.Success)
            {
                return Json(result.Data);
            }

            _logger.Log(LogLevel.Error,result.Message);
            return Json(new {isSuccess = false , message = result.Message});

        }

        #endregion

        #region public async Task<JsonResult> AssertionOptions([FromBody] AssertionOptionsViewModel assertionOptionsViewModel)

        [HttpPost]
        [Route("/assertionOptions")]
        public async Task<JsonResult> AssertionOptions([FromBody] AssertionOptionsViewModel assertionOptionsViewModel)
        {
            var result = await _authenticationService.AssertionOptions(new AssertionOptionsRequestDto
            {
                Username = assertionOptionsViewModel.Username,
                UserVerification = assertionOptionsViewModel.UserVerification
            });

            if (result.Status == StatusTypeEnum.Success)
            {
                // Temporarily store options, session/in-memory cache/redis/db 
                var cookieOptions = new CookieOptions()
                {
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddMinutes(2),
                    HttpOnly = true,
                };
                var content = _protector.Protect(result.Data.ToJson());
                HttpContext?.Response.Cookies.Append("fido2.assertionOptions", content, cookieOptions);

                // return options to client
                return Json(result.Data);
            }
            _logger.Log(LogLevel.Error,result.Message);
            return Json(new {isSuccess = false , message = result.Message});
        }

        #endregion

        #region public async Task<IActionResult> MakeAssertion([FromBody] MakeAssertionViewModel makeAssertionViewModel)

        [HttpPost]
        [Route("/makeAssertion")]
        public async Task<IActionResult> MakeAssertion([FromBody] MakeAssertionViewModel makeAssertionViewModel)
        {
            // 1. get the options we sent the client
            if (string.IsNullOrEmpty(HttpContext?.Request.Cookies["fido2.assertionOptions"]))
            {
                _logger.Log(LogLevel.Error,"AssertionOptions not found!");
                return Json(new {isSuccess = false , message = "AssertionOptions not found!"});
            }
            
            var jsonOptions = _protector.Unprotect(HttpContext?.Request.Cookies["fido2.assertionOptions"]!);
            var options = Fido2NetLib.AssertionOptions.FromJson(jsonOptions);

            var result = await _authenticationService.MakeAssertion(new MakeAssertionRequestDto
            {
                AuthenticatorAssertionRawResponse = makeAssertionViewModel.AuthenticatorAssertionRawResponse,
                AssertionOption = options
            });
            
            if (result.Status == StatusTypeEnum.Success && result.Data.Status.Equals("ok"))
            {
                return Json(result.Data);
            }

            _logger.Log(LogLevel.Error,result.Message);
            return Json(new {isSuccess = false , message = result.Message});

        }

        #endregion
        
    }
}