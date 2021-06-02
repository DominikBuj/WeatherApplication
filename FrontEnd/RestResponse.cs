using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FrontEnd
{
    class RestResponse
    {
        [Required]
        public string AverageValue { get; set; }
    }
}
