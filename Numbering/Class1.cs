using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Security_Check;

using Reference = Autodesk.Revit.DB.Reference;
using Exceptions = Autodesk.Revit.Exceptions;

namespace Numbering
{ 
    //Transaction assigned as automatic
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    //Creating an external command to annotate the structure elements
    public class Numbering : IExternalCommand
    {

        //instances to store application and the document
        UIDocument m_document;
        UIApplication m_application;

        bool isNumber = false;
        public static int startNum = -1, option=-1;
        private static float tolerance = -1;

        bool security = false;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                //call to the security check method to check for authentication
                security=SecurityLNT.Security_Check();
               
                if (security == false)
                {
                    return Result.Succeeded;
                }

                //the application and document has been initialised
                m_application = commandData.Application;
                m_document = m_application.ActiveUIDocument;

                //Revit task dialog to communicate information to user.
                TaskDialog mainDialog = new TaskDialog("Numbering Tool");
                mainDialog.MainInstruction = "Numbering Tool";
                mainDialog.MainContent =
                    "Please choose from the following options:\n";

                //Add commmandLink to task dialog
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1,
                                          "Auto Numbering (Rectangular Builing) ");
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2,
                                          "Manual Numbering (Curved/Irregular Building)");

                //Buttons to be shown on main dialogue window.
                mainDialog.CommonButtons = TaskDialogCommonButtons.Close;
                mainDialog.DefaultButton = TaskDialogResult.Close;
            
                TaskDialogResult tResult = mainDialog.Show();

                //conditions for options chosen by the user
                if (TaskDialogResult.CommandLink1 == tResult)
                {
                    IList<Element> selectedElementTags = new List<Element>();
                    IList<Element> selectedElements = new List<Element>();

                    //element selection. all elements have to be picked and then press finish to continue with auto numbering.
                    //show dialogue
                    using (NumberAndAlignment numberAndAlignmentInstance = new NumberAndAlignment())
                    {
                        bool loopControl = true;
                        while (loopControl)
                        {
                            DialogResult dialogueResult = numberAndAlignmentInstance.ShowDialog();
                            if (DialogResult.OK == dialogueResult)
                            {
                                isNumber = numberAndAlignmentInstance.CheckStartNumber();
                                if (isNumber)
                                {
                                    startNum = numberAndAlignmentInstance.ReturnStartNumber();
                                    tolerance = 3.28f * numberAndAlignmentInstance.ReturnTolerance();
                                    option = numberAndAlignmentInstance.ReturnNumberingChoice();
                                    loopControl = false;
                                }
                                else
                                {
                                    MessageBox.Show("Enter Integer for Start Number and Decimal for Tolerance.");
                                }
                            }
                            else if (DialogResult.Abort == dialogueResult)
                                loopControl = false;
                        }
                        if (startNum != -1 && option != -1)
                        {
                            loopControl = true;
                            selectedElementTags.Clear();
                            
                            while (loopControl)
                            {
                                ((ICollection<Element>)selectedElementTags).Clear();
                                try
                                {
                                    selectedElementTags = m_document.Selection.PickElementsByRectangle(new TagSelectionFilter(m_document.Document), "Select Elements by a Selection Rectangle.");
                                    if (((ICollection<Element>)selectedElementTags).Count > 0)
                                        loopControl = false;
                                }
                                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                                {
                                    loopControl = false;
                                }
                            }

                            if (option == 1)
                            {
                                selectedElements.Clear();
                                foreach(Element ele in selectedElementTags)
                                {
                                    selectedElements.Add(((IndependentTag)ele).GetTaggedLocalElement());
                                }

                                selectedElementTags.Clear();
                                HorizontalLtoR(selectedElements);
                                selectedElements.Clear();
                            }
                            else if (option == 2)
                            {
                                selectedElements.Clear();
                                foreach (Element ele in selectedElementTags)
                                {
                                    selectedElements.Add(((IndependentTag)ele).GetTaggedLocalElement());
                                }

                                selectedElementTags.Clear();
                                VerticalTtoB(selectedElements);
                                selectedElements.Clear();
                            }

                        }

                    }
                }           
                else if (TaskDialogResult.CommandLink2 == tResult)
                {
                    //show dialogue
                    using (StartNumber startNumberInstance = new StartNumber())
                    {
                        bool control = true;
                        while (control)
                        {
                            DialogResult dialogueResult = startNumberInstance.ShowDialog();
                            if (DialogResult.OK == dialogueResult)
                            {
                                isNumber = startNumberInstance.CheckStartNumber();
                                if (isNumber)
                                {
                                    startNum = startNumberInstance.ReturnStartNumber();
                                    control = false;
                                }
                                else
                                {
                                    MessageBox.Show("Please Enter a Number.");
                                }
                            }
                            else if (DialogResult.Abort == dialogueResult)
                                control = false;
                            else if (DialogResult.Cancel == dialogueResult)
                                control = false;
                        }

                    }

                    if (startNum != -1)
                    {

                        //element selection. the element has to be picked in the order in which it has to be numbered.
                        //once the selection is completed press ESC to end the selection
                        Reference elementReference = null;

                        bool continueLoop = true;

                        while (continueLoop)
                        {
                            try
                            {
                                elementReference = m_document.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element
                                    , new TagSelectionFilter(m_document.Document), "Pick elements to change Mark in the required order. Press ESC to END Slections.");
                                if (ChangeMarkValue.GetTaggedElement(m_document, elementReference, startNum))
                                    ;
                                else
                                    return Autodesk.Revit.UI.Result.Failed;
                            }
                            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                            {
                                continueLoop = false;
                            }

                            startNum++;
                        }
                    }                
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Autodesk.Revit.UI.Result.Failed;
            }
            throw new NotImplementedException();

        }

        private void HorizontalLtoR(IList<Element> selectedElementsLocal)
        {
            IList<Element> tempList = new List<Element>();
            XYZ xyz = new XYZ();

            while (selectedElementsLocal.Count != 0)
            {
                xyz = ((LocationPoint)selectedElementsLocal.First().Location).Point;

                foreach (Element ele in selectedElementsLocal)
                {
                    if(((LocationPoint)ele.Location).Point.Y >= xyz.Y)
                        xyz=((LocationPoint)ele.Location).Point;
                }

                foreach (Element ele in selectedElementsLocal)
                {
                    if(((float)xyz.Y-tolerance)<= (float)((LocationPoint)ele.Location).Point.Y)
                    {
                        tempList.Add(ele);
                    }
                }

                while (tempList.Count != 0)
                {
                    Element localTaggedElement = tempList.First();

                    foreach(Element ele in tempList)
                    {
                        if (((LocationPoint)ele.Location).Point.X <= ((LocationPoint)localTaggedElement.Location).Point.X)
                        {
                            localTaggedElement = ele;
                        }
                    }

                    ChangeMarkValue.MarkValue(localTaggedElement, startNum);
                    ++startNum;
                    tempList.Remove(localTaggedElement);
                    selectedElementsLocal.Remove(localTaggedElement);
                }
            }
        }

        private void VerticalTtoB(IList<Element> selectedElementsLocal)
        {
            IList<Element> tempList = new List<Element>();
            XYZ xyz = new XYZ();

            while (selectedElementsLocal.Count != 0)
            {
                xyz = ((LocationPoint)selectedElementsLocal.First().Location).Point;

                foreach (Element ele in selectedElementsLocal)
                {
                    if (((LocationPoint)ele.Location).Point.X <= xyz.X)
                        xyz = ((LocationPoint)ele.Location).Point;
                }

                foreach (Element ele in selectedElementsLocal)
                {
                    if ((float)((LocationPoint)ele.Location).Point.X <= ((float)xyz.X + tolerance))
                    {
                        tempList.Add(ele);
                    }
                }

                while (tempList.Count != 0)
                {
                    Element localTaggedElement = tempList.First();

                    foreach (Element ele in tempList)
                    {
                        if (((LocationPoint)ele.Location).Point.Y >= ((LocationPoint)localTaggedElement.Location).Point.Y)
                        {
                            localTaggedElement = ele;
                        }
                    }

                    ChangeMarkValue.MarkValue(localTaggedElement, startNum);
                    ++startNum;
                    tempList.Remove(localTaggedElement);
                    selectedElementsLocal.Remove(localTaggedElement);
                }
            }
        }
    }
    
    
    //static class to change the mark value
    public static class ChangeMarkValue
    {        
        private static int startNumber=0;

        //method to find the tagged element using the reference
        public static bool GetTaggedElement(UIDocument m_doc ,Reference LocalRef, int num)
        {
            startNumber=num;
            IndependentTag selectedTag = null;
            Element taggedElement = null;
                      
            //make sure that the ref is not null
            if (LocalRef != null)
            {
                //check if the element selected is of type independent tag
                if (m_doc.Document.GetElement(LocalRef) is IndependentTag)
                {
                    selectedTag = (IndependentTag)m_doc.Document.GetElement(LocalRef);

                    //find the element which is tagged by the selected tag
                    taggedElement = selectedTag.GetTaggedLocalElement();

                    //call to the change mark value method
                    if (MarkValue(taggedElement))
                        return true;
                    else
                        return false;

                }
            }
            return false;                  
        }

        //method to change thenmark value of the tagged element
        public static bool MarkValue(Element localTaggedElement, int num = -1)
        {
            if (num != -1)
                ChangeMarkValue.startNumber = num;

            //find the mark parameter of the tagged element and set a new value to it
            foreach (Parameter param in localTaggedElement.Parameters)
            {
                if (param.Definition.Name.ToString() == "Mark")
                {
                    try
                    {
                        param.Set(startNumber.ToString());
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
