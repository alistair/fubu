using System.ComponentModel;

namespace Fubu.Generation
{
    /*
     * 
     * 1.) Brand new solution        fubu new [solution name] | --project fubumvc-app --list
     * 2.) alter a solution (?)      
     * 3.) new project               fubu new-project [project name] [template] --options [option1] [option2] --testing --list
     * 4.) alter project             fubu alter-project [project name] [template1] [template2]  --list
     * 5.) new testing project       fubu add-test-project [project name]
     */

    public class NewCommandInput
    {
        public NewCommandInput()
        {
            RippleFlag = FeedChoice.PublicOnly;
        }


        // If this is blank, use this folder as the solution name too
        public string SolutionName { get; set; }

        [Description("If chosen, fubu new will also create a single, empty FubuMVC application project with the same name")]
        public bool AppFlag { get; set; }

        public bool CleanFlag { get; set; }

//        [FlagAlias("output", 'o')]
//        [Description("The output directory if different than the project name")]
//        public string OutputFlag { get; set; }

//        [FlagAlias("solution", 's')]
//        [Description("The Visual Studio solution file to modify to include templated projects")]
//        public string SolutionFlag { get; set; }

//        [Description("The name and destination folder of the new FubuMVC project")]
//        public string ProjectName { get; set; }

        public FeedChoice RippleFlag { get; set; }
    }

    public enum FeedChoice
    {
        FloatingEdge,
        Edge,
        PublicOnly
    }
}