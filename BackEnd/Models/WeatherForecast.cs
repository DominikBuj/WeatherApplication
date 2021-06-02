using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Models
{
    public class WeatherForecast
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public float Temperature { get; set; }
        [Required]
        public float Pressure { get; set; }
        [Required]
        public float Humidity { get; set; }
        [Required]
        public float Precipitation { get; set; }
        [Required]
        public float WindSpeed { get; set; }
        [Required]
        public float WindDirection { get; set; }

    }
}
