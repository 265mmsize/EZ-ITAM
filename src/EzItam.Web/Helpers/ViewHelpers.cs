using EzItam.Web.Models.Entities;
using EzItam.Web.Services;
using Microsoft.AspNetCore.Html;

namespace EzItam.Web.Helpers;

public static class ViewHelpers
{
    public static string Display(this Enum value) => EnumDisplay.Name(value);

    public static IHtmlContent StatusBadge(this AssetStatus status)
    {
        var css = status switch
        {
            AssetStatus.InStock => "bg-success",
            AssetStatus.Assigned => "bg-primary",
            AssetStatus.InRepair => "bg-warning text-dark",
            AssetStatus.Idle => "bg-info text-dark",
            AssetStatus.Retired => "bg-secondary",
            AssetStatus.Disposed => "bg-dark",
            AssetStatus.Lost => "bg-danger",
            _ => "bg-secondary"
        };
        return new HtmlString($"<span class=\"badge {css}\">{EnumDisplay.Name(status)}</span>");
    }

    public static string Money(this decimal? value) => value.HasValue ? value.Value.ToString("#,##0") + "원" : "-";
    public static string Money(this decimal value) => value.ToString("#,##0") + "원";
    public static string Ymd(this DateTime? value) => value?.ToString("yyyy-MM-dd") ?? "-";
    public static string Ymd(this DateTime value) => value.ToString("yyyy-MM-dd");
    public static string Ymdhm(this DateTime? value) => value?.ToString("yyyy-MM-dd HH:mm") ?? "-";
    public static string Ymdhm(this DateTime value) => value.ToString("yyyy-MM-dd HH:mm");
}
