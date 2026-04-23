using System;
var me = Process.Self;

Status.Throw(1, me, "Shell Loaded");

//repl logic goes here



Process.Send(new Message("RegisterKeyEvent", @"[\x00-\x7F]", me));

Status.Throw(1, me, "New shell: " + me.GUID.ToString());
while (true)
{
	if (me.Messages.TryDequeue(out var message))
	{
		Terminal.SetRow(20, message.Key + "(" + message.Timestamp + "): " + message.Value + "\n");
		Status.Throw(0, me, message.Key + "(" + message.Timestamp + "): " + message.Value + "\n");
	}
}