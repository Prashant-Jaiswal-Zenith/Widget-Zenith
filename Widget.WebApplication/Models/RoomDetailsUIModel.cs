using System.ComponentModel.DataAnnotations;

namespace Widget.WebApplication.Models {
    public class RoomDetailsUIModel {
        [Display(Name = "Room No:")]
        public int RoomNo { get; set; }

        [Display(Name = "No Of Adults:")]
        public int NoOfAdults { get; set; }

        [Display(Name = "No Of Children:")]
        public int NoOfChildren { get; set; }
    }
}