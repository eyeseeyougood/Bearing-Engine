using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Bearing;
using OpenTK.Mathematics;

public class InspectorItem : Component
{
    public GameObject linkedObject;
    public int compId;

    private Component comp;

    private UIVerticalScrollView scrollView;

    public override void Cleanup()
    {
    }

    public override void OnLoad()
    {
        // init scroll
        scrollView = new UIVerticalScrollView();
        scrollView.renderLayer = -1;
        scrollView.metadata = new object[] { 5 };
        scrollView.scrollSensitivity = 0;
        scrollView.consumedInputs.Remove("Scroll");
        scrollView.theme = new UITheme() { verticalScrollBG = new BearingColour() { zeroToOne = new Vector4(0.65f, 0.65f, 0.65f, 1f) } };
        scrollView.size = new UDim2(0, 0, 0, 100);
        linkedObject.AddComponent(scrollView);
        ((UIVerticalScrollView)UIManager.FindFromID(2)).contents.Add(scrollView.rid);

        // get the linked component
        comp = linkedObject.GetComponent(compId);

        if (comp == null)
        {
            Logger.LogError("Invalid component or linked object!");
            return;
        }
        
        // generate all property UI
        foreach (var property in comp.GetType().GetProperties())
        {
            // temporary text labels, later replace with actual interactable UI stuffs XD
            for (int i = 0; i < 2; i++)
            {
                UILabel propertyLabel = new UILabel();
                propertyLabel.text = property.Name;
                propertyLabel.size = new UDim2(0, 0, 0, 100);
                gameObject.AddComponent(propertyLabel);

                scrollView.contents.Add(propertyLabel.rid);
            }
        }

        scrollView.size = new UDim2(0,0,0,100* scrollView.contents.Count + scrollView.spacing*(scrollView.contents.Count-1));
    }

    public override void OnTick(float dt)
    {
    }
}