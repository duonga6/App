using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace App.Menu
{

    public enum SidebarItemType
    {
        Divider,
        Heading,
        NavItem
    }

    public class SidebarItem
    {

        public string Title { set; get; }

        public bool IsActive { set; get; }

        public SidebarItemType Type { set; get; }

        public string Action { set; get; }

        public string Controller { set; get; }

        public string Area { set; get; }

        public string AwesomeIcon { set; get; }  // Tên class thẻ <i>

        public List<SidebarItem> Items { set; get; }

        public string CollapseID { set; get; }

        public string GetLink(IUrlHelper urlHelper)
        {
            return urlHelper.Action(Action, Controller, new { area = Area });
        }

        public string RenderHtml(IUrlHelper urlHelper)
        {
            var html = new StringBuilder();

            if (Type == SidebarItemType.Divider)
            {
                html.Append("<hr class=\"sidebar-divider my-0\">");
            }
            else if (Type == SidebarItemType.Heading)
            {
                html.Append(@$"
                    <div class=""sidebar-heading my-2"">
                        {Title}
                    </div>
                ");
            }
            else if (Type == SidebarItemType.NavItem) {
                if (Items == null)
                {
                    var url = GetLink(urlHelper);
                    var icon = AwesomeIcon != null ? $"<i class=\"{AwesomeIcon}\"></i>" : "";
                    var active = IsActive ? "active" : "";

                    html.Append(@$"
                        <li class=""nav-item {active}"">
                            <a class=""nav-link"" href=""{url}"">
                                {icon}
                                <span>{Title}</span>
                            </a>
                        </li>
                    ");
                }
                else
                {
                    var icon = AwesomeIcon != null ? $"<i class=\"{AwesomeIcon}\"></i>" : "";
                    var active = IsActive ? "active" : "";
                    var showItem = IsActive ? "show" : "";
                    
                    var itemList = new StringBuilder();
                    foreach (var item in Items)
                    {
                        var url = item.GetLink(urlHelper);
                        var activeClass = item.IsActive ? "active" : "";
                        itemList.Append($"<a class=\"collapse-item {activeClass}\" href=\"{url}\">{item.Title}</a>");
                    }

                    html.Append(@$"
                        <li class=""nav-item {active}"">
                            <a class=""nav-link collapsed"" href=""#"" data-toggle=""collapse"" data-target=""#{CollapseID}""
                            aria-expanded=""true"" aria-controls=""{CollapseID}"">
                                {icon}
                                <span>{Title}</span>
                             </a>
                            <div id=""{CollapseID}"" class=""collapse {showItem}"" aria-labelledby=""headingTwo""
                                data-parent=""#accordionSidebar"">
                                <div class=""bg-white py-2 collapse-inner rounded"">
                                    {itemList.ToString()}
                                </div>
                            </div>
                        </li>
                    ");
                }
            }

            return html.ToString();
        }
    }
}