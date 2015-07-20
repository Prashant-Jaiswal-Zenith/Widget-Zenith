using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Widget.WebApplication.Models {
    public class PropertySearchParameterUIModel {
        public PropertySearchParameterUIModel() {
            Rooms = new HashSet<RoomDetailsUIModel>();
        }

        [Required(ErrorMessage = "Please enter city to search")]
        [Display(Name = "Enter City:")]
        public string CityName { get; set; }

        public int CityId { get; set; }

        [Required(ErrorMessage = "Please enter check in date")]
        [Display(Name = "Check-In:")]
        public DateTime? CheckIn { get; set; }

        [Required(ErrorMessage = "Please enter check out date")]
        [Display(Name = "Check-Out:")]
        public DateTime? CheckOut { get; set; }

        [Required(ErrorMessage = "Please enter number of rooms")]
        [Display(Name = "Rooms:")]
        public int NoOfRooms { get; set; }

        public ICollection<RoomDetailsUIModel> Rooms { get; set; }
    }
}