public void Main(string arg)
{
    // If setupcomplete is false, run Setup method.
    if (setupcomplete == false)
    {
        Echo("Running setup.");
        Setup();
    }
    else
    {
        // Script magic will happen here.

        // Create a tag. Our friend will use this in his script in order to receive our messages.
        string tag1 = "channel 1";

        // Create our message. We first make it a string, and then we "box" it as an object type.                                                 
        string messageOut = "Hello friend!";

        // Through the IGC variable we issue the broadcast method. IGC is "pre-made",
        // so we don't have to declare it ourselves, just go ahead and use it. 
        IGC.SendBroadcastMessage(tag1, messageOut, TransmissionDistance.TransmissionDistanceMax);

        // To create a listener, we use IGC to access the relevant method. 
        // We pass the same tag argument we used for our message. 
        IGC.RegisterBroadcastListener(tag1);

        // Create a list for broadcast listeners.
        List<IMyBroadcastListener> listeners = new List<IMyBroadcastListener>();

        // The method argument below is the list we wish IGC to populate with all Listeners we've made.
        // Our Listener will be at index 0, since it's the only one we've made so far.
        IGC.GetBroadcastListeners(listeners);

        // We'll use the Listener property HasPendingMessage, it returns true if we have unread messages.
        if (listeners[0].HasPendingMessage)
        {
            // Let's create a variable for our new message. 
            // Remember, messages have the type MyIGCMessage.
            MyIGCMessage message = new MyIGCMessage();

            // Time to get our message from our Listener (at index 0 of our Listener list). 
            // We do this with the following method:
            message = listeners[0].AcceptMessage();

            // A message is a struct of 3 variables. To read the actual data, 
            // we access the Data field, convert it to type string (unboxing),
            // and store it in the variable messagetext.
            string messagetext = message.Data.ToString();

            // We can also access the tag that the message was sent with.
            string messagetag = message.Tag;

            //Here we store the "address" to the Programmable Block (our friend's) that sent the message.
            long sender = message.Source;

            //Do something with the information!
            Echo("Message received with tag" + messagetag + "\n\r");
            Echo("from address " + sender.ToString() + ": \n\r");
            Echo(messagetext);
        }

        // The unicast Listener is pre-made and accessed through IGC. 
        // We store it as unisource for easy access.
        IMyUnicastListener unisource = IGC.UnicastListener;

        // Check if we have new messages.
        if (unisource.HasPendingMessage)
        {
            // Just like earlier, we create a variable for our message and accept the new 
            // message from our Listener. We do the message unboxing as we write it out.
            MyIGCMessage messageUni = unisource.AcceptMessage();
            Echo("Unicast received from address " + messageUni.Source.ToString() + "\n\r");
            Echo("Tag: " + messageUni.Tag + "\n\r");
            Echo("Data: " + messageUni.Data.ToString());
        }

        // To unicast a message to our friend, we need an address for his Programmable Block.
        // We'll pretend here that he has copied it and sent it to us via Steam chat.
        long friendAddress = 3672132753819237;

        // Here, we'll use the tag to convey information about what we're sending to our friend.
        string tagUni = "Int";

        // We're sending a number instead of a string. We box it in the same way as before.
        int number = 1337;

        // We access the unicast method through IGC and input our address, tag and data.
        IGC.SendUnicastMessage(friendAddress, tagUni, number);
    }
}
