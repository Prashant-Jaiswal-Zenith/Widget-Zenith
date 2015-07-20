using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Widget.WebApplication.Models;

namespace Widget.WebApplication.Controllers
{
    public class WidgetController : Controller
    {
        public async Task<PartialViewResult> SearchHotelPartial() {
            await Task.Run(() => CreateSearchViewBag(10, 3));

            var model = new PropertySearchParameterUIModel();

            model.NoOfRooms = 1;
            model.Rooms = CreateEmptyRooms(10).ToList();

            return PartialView(model);
        }

        private void CreateSearchViewBag(int maxNoOfRooms, int maxPaxCount) {
            ViewBag.MaxRooms = maxNoOfRooms;

            var list = Enumerable.Range(1, maxNoOfRooms).
                Select(n => new SelectListItem {
                    Text = n.ToString(),
                    Value = n.ToString()
                }).ToList();
            ViewBag.Rooms = list;

            list = Enumerable.Range(1, maxPaxCount).
                Select(n => new SelectListItem {
                    Text = n.ToString(),
                    Value = n.ToString()
                }).ToList();
            ViewBag.Adults = list;

            list = Enumerable.Range(0, maxPaxCount).
                Select(n => new SelectListItem {
                    Text = n.ToString(),
                    Value = n.ToString()
                }).ToList();
            ViewBag.Children = list;
        }

        private IEnumerable<RoomDetailsUIModel> CreateEmptyRooms(int maxNoOfRooms) {
            foreach (var room in Enumerable.Range(1, maxNoOfRooms))
                yield return new RoomDetailsUIModel {
                    RoomNo = room,
                    NoOfAdults = 1,
                    NoOfChildren = 0
                };
        }
    }
}