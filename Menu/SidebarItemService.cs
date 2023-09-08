using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace App.Menu
{
    public class SidebarItemService
    {
        private readonly IUrlHelper urlHelper;

        public List<SidebarItem> Items { set; get; } = new();

        public SidebarItemService(IUrlHelperFactory factory, IActionContextAccessor accessor)
        {
            urlHelper = factory.GetUrlHelper(accessor.ActionContext);

            // Khởi tạo các mục Sidebar

            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });
            Items.Add(new SidebarItem() { Type = SidebarItemType.Heading, Title = "Quản lý chung" });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Quản lý database",
                Controller = "DbManager",
                Action = "Index",
                Area = "Database",
                AwesomeIcon = "fas fa-database"
            });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Quản lý liên hệ",
                Controller = "Contact",
                Action = "Index",
                Area = "Contact",
                AwesomeIcon = "fas fa-id-badge"
            });

            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Phân quyền & tài khoản",
                AwesomeIcon = "fas fa-folder",
                CollapseID = "role",
                Items = new List<SidebarItem>() {
                    new() {
                        Type = SidebarItemType.NavItem,
                        Title = "Quản lý thành viên",
                        Controller = "User",
                        Action = "Index",
                        Area = "Identity",
                    },
                    new() {
                        Type = SidebarItemType.NavItem,
                        Title = "Quản lý vai trò",
                        Controller = "Role",
                        Action = "Index",
                        Area = "Identity",
                    }
                }
            });

            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Quản lý bài viết",
                AwesomeIcon = "fas fa-mail-bulk",
                CollapseID = "blog",
                Items = new List<SidebarItem>() {
                    new() {
                        Type = SidebarItemType.NavItem,
                        Title = "Quản lý danh mục",
                        Controller = "Category",
                        Action = "Index",
                        Area = "Blog",
                    },
                    new() {
                        Type = SidebarItemType.NavItem,
                        Title = "Quản lý bài viết",
                        Controller = "Post",
                        Action = "Index",
                        Area = "Blog",
                    }
                }
            });

            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Quản lý sản phẩm",
                AwesomeIcon = "fab fa-product-hunt",
                CollapseID = "product",
                Items = new List<SidebarItem>() {
                    new() {
                        Type = SidebarItemType.NavItem,
                        Title = "Quản lý danh mục",
                        Controller = "CategoryProduct",
                        Action = "Index",
                        Area = "Product",
                    },
                    new() {
                        Type = SidebarItemType.NavItem,
                        Title = "Quản lý sản phẩm",
                        Controller = "ProductManager",
                        Action = "Index",
                        Area = "Product",
                    }
                }
            });

            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Quản lý File",
                Controller = "FileManager",
                Action = "Index",
                Area = "Files",
                AwesomeIcon = "fas fa-file"
            });

        }

        public string RenderHtml()
        {
            var html = new StringBuilder();

            foreach (var item in Items)
            {
                html.Append(item.RenderHtml(urlHelper));
            }

            return html.ToString();
        }

        public void SetActive(string Controller, string Area)
        {
            foreach (var item in Items)
            {
                if (item.Controller == Controller && item.Area == Area)
                {
                    item.IsActive = true;
                    return;
                }
                else
                {
                    if (item.Items != null)
                    {
                        foreach (var childItem in item.Items)
                        {
                            if (childItem.Controller == Controller && childItem.Area == Area)
                            {
                                item.IsActive = true;
                                childItem.IsActive = true;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}