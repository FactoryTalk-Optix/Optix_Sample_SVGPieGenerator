#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.NativeUI;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using System.Text;
using System.Xml.Linq;
#endregion

public class PieManager : BaseNetLogic
{
    private IUAVariable slices;
    private IUAVariable xml;
    private IUAVariable diameter;

    private string pieXML = "<svg height=\"20\" width=\"20\" viewBox=\"0 0 20 20\"><circle r=\"10\" cx=\"10\" cy=\"10\" fill=\"lime\" /></svg>";

    public override void Start()
    {
        slices = LogicObject.GetVariable("slices");
        xml = LogicObject.GetVariable("xml");
        diameter = LogicObject.GetVariable("diameter");

    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void GeneratePie()
    {
        const double outerR = 10;     // radius of background circle
        const double ringR = 5;       // radius of the “slice ring”
        const double strokeWidth = 10;
        double circumference = 2 * Math.PI * ringR;
        double dash = circumference / slices.Value;    // length of each “slice”
        double gap = circumference;                   // gap = full circle
        double angleStep = 360.0 / slices.Value;

        // A simple palette – will cycle if slices > palette.Length
        string[] palette = new[]
        {
            "tomato", "mediumseagreen", "steelblue",
            "gold", "orchid", "coral", "slateblue",
            "seagreen", "crimson", "orange",
            "tomato", "mediumseagreen", "steelblue",
            "gold", "orchid", "coral", "slateblue",
            "seagreen", "crimson", "orange",
            "tomato", "mediumseagreen", "steelblue",
            "gold", "orchid", "coral", "slateblue",
            "seagreen", "crimson", "orange"
        };

        XNamespace svgNs = "http://www.w3.org/2000/svg";

        var svg = new XElement(svgNs + "svg",
            new XAttribute("width", 100),
            new XAttribute("height", 100),
            new XAttribute("viewBox", "0 0 20 20"),

            // Background disk:
            new XElement(svgNs + "circle",
                new XAttribute("r", outerR),
                new XAttribute("cx", outerR),
                new XAttribute("cy", outerR),
                new XAttribute("fill", "lime")
            )
        );

        // Overlay each slice
        for (int i = 0; i < slices.Value; i++)
        {
            double rotate = -90 + i * angleStep;
            string color = palette[i % palette.Length];

            svg.Add(new XElement(svgNs + "circle",
                new XAttribute("r", ringR),
                new XAttribute("cx", outerR),
                new XAttribute("cy", outerR),
                new XAttribute("fill", "transparent"),
                new XAttribute("stroke", color),
                new XAttribute("stroke-width", strokeWidth),
                new XAttribute("stroke-dasharray", $"{dash.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture)} {gap.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture)}"),
            // Rotate around the center (10,10)
                new XAttribute("transform", $"rotate({rotate.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture)} {outerR} {outerR})")
            ));
        }

        // Prepend XML prolog
        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            svg
        );

        xml.Value = doc.ToString();
    }
}
