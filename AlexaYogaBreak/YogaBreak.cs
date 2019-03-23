namespace AlexaYogaBreak
{
    public class YogaBreak
    {
        public string Locale { get; set; }
        public string SkillName { get; set; }
        public string LaunchMessage { get; set; }
        public string LaunchMessageReprompt { get; set; }
        public string StopMessage { get; set; }
        public string HelpMessage { get; set; }
        public string HelpReprompt { get; set; }

        public string StartSessionMessage { get; set; }
        public string FirstPose { get; set; }

        public string SecondPose { get; set; }

        public string ThirdPose { get; set; }

        public string FourthPose { get; set; }

        public string FifthPose { get; set; }

        public string SixthPose { get; set; }

        public string SeventhPose { get; set; }

        public string EighthPose { get; set; }

        public string NinthPose { get; set; }

        public YogaBreak(string locale)
        {
            this.Locale = locale;
        }

        public static YogaBreak NewSession()
        {
            YogaBreak thisSession = new YogaBreak("en-US");

            thisSession.LaunchMessage = "Welcome to Yoga Break. This is a quick, guided yoga session to help you relax. When you're on your mat and ready, say begin session ";
            thisSession.LaunchMessageReprompt = "Try asking me to start the yoga break session ";
            thisSession.SkillName = "Yoga Break";
            thisSession.HelpMessage = "Yoga Break is a quick, guided yoga session. Follow along with my prompts. When you're ready, say begin session ";
            thisSession.HelpReprompt = "What can I help you with? ";
            thisSession.StopMessage = "Thank you for using Yoga Break!";

            thisSession.StartSessionMessage = "Let's begin. Remember to use your breathing and there is a ten second pause between poses ";
            thisSession.FirstPose = "The first pose is a mountain pose...standing tall with your hands over your head" + @"<break time=""1s""/>" + " stretch! " + @"<break time=""10s""/>";
            thisSession.SecondPose = "Now move into a forward fold with your head down and your hands close to the floor...it's ok if you can't reach...bend your knees if you need " + @"<break time=""10s""/>";
            thisSession.ThirdPose = "As you breathe in, come up for a half way lift...think of your back being a flat table and really get a stretch in your neck " + @"<break time=""10s""/>";
            thisSession.FourthPose = "Move back down into a forward fold, touching your hands to the ground...shake it out if you want by moving your head up and down or back and forth...you can also swing your arms or let them hang " + @"<break time=""10s""/>";
            thisSession.FifthPose = "Now plant your palms and step it back or jump into a plank pose...really think about pushing away from the ground with your palms flat..if you find holding a plank pose difficult, you can put your knees on the floor " + @"<break time=""10s""/>";
            thisSession.SixthPose = "We're ready for the first down dog...make sure your palms are flat as you push your hips up...your heels can be up or flat on the ground, whichever is comfortable for you " + @"<break time=""10s""/>";
            thisSession.SeventhPose = "Look forward between your palms and step or hop back up to your forward fold position " + @"<break time=""10s""/>";
            thisSession.EighthPose = "Slowly roll it back up...very slowly...feeling each part of your spine unbend as you come up to your mountain pose with hands overhead " + @"<break time=""10s""/>";
            thisSession.NinthPose = "Now bring your hands down to your heart...take a deep, cleansing breath in as you thank yourself for taking this yoga break...and let that breath out all the way as a big sigh " + @"<break time=""4s""/>";

            return thisSession;
        }
    }
}
