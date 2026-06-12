using Microsoft.AspNetCore.Mvc.Razor;

namespace Peer_Car.Presentation.PresentationViewLocationExpander
{
    public class PresentationViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context) { }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            string[] presentationLocations = new[]
            {
        // 1. المسارات العادية (Home, Account, Cars)
        "/Presentation/Views/{1}/{0}.cshtml",
        
        // 2. مسار الـ Admin (للفولدرات الفرعية)
        "/Presentation/Views/Admin/{1}/{0}.cshtml", 

        // 3. المسارات المشتركة (Shared) 
        // السطر ده هو اللي هيشغل الـ ViewComponents صح من غير تكرار
        "/Presentation/Views/Shared/{0}.cshtml"
    };

            return presentationLocations.Union(viewLocations);
        }
    }
}
