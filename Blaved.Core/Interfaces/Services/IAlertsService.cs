using Blaved.Core.Objects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaved.Core.Interfaces.Services
{
    public interface IAlertsService
    {
        Task WithdrawCompletedAlert(WithdrawModel transaction);
        Task DepositCompletedAlert(DepositModel depositModel);
    }
}
