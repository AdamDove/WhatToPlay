using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatToPlay.Model
{
    public interface ISteamGuardPromptHandler
    {
        string GetEmailAuthenticationCode();
        string GetTwoFactorAuthenticationCode();
    }
}
