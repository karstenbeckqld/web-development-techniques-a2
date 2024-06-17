using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System;
using System.Reflection;

namespace MCBA.Enums
{
    public enum StateEnum
    {
        [Display(Name = "none")] NO,

        [Display(Name = "Queensland")] QLD,

        [Display(Name = "South Australia")] SA,

        [Display(Name = "Victoria")] VIC,

        [Display(Name = "Northern Territory")] NT,

        [Display(Name = "Australian Capital Territory")] ACT,

        [Display(Name = "Western Australia")] WA,

        [Display(Name = "Tasmania")] TAS
    }

    public enum billPayEnum
    {
        [Display(Name = "Once off")] O,
        [Display(Name = "Monthly")] M
    }
}