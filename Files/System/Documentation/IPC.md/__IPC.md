IPC messages use the `Message` class.

Simple example to register for keypress events (though it is recommended to use the input event system):  
First we send our request to the kernel. Here we ask for all ASCII characters using a regex.
```c#
Process.Send(new Message("RegisterKeyEvent", @"[\x00-\x7F]", me));
``` 
Currently, you won't get any confirmation response, but implementations *should* provide one for integrity.

Now we can read messages!
```c#
if (me.Messages.TryDequeue(out var message))
{
	Terminal.SetRow(20, message.Key + "(" + message.Timestamp + "): " + message.Value + "\n");
	Status.Throw(0, me, message.Key + "(" + message.Timestamp + "): " + message.Value + "\n");
}
```
In case you haven't used concurrent collections, `Messages.TryDequeue` only returns `true` if there's something to dequeue. 