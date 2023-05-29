# VisualC2Handler
Visual Basic Multi Handler

Like [VBCat](https://github.com/MidwintersTomb/VBCat), this started out as a bit of a shitposter style joke when a friend said we should make a C2 together, and I joked we should make it in VB, but since I'm a terrible programmer, see if I can get ChatGPT to write the code, and then do any corrections, tweaks, etc. that I need to to get it work as intended.  This is the second half ([VBCat](https://github.com/MidwintersTomb/VBCat) being the first half), a completed VB.Net multihandler.

So, that's exactly what this is.  I tossed a base idea at ChatGPT, which it mangled to all hell, but had a good idea in using lists and dictionaries for separating everything out.  I then took what I learned from creating [VBCat](https://github.com/MidwintersTomb/VBCat), as well as learning via the interwebz as I worked on this, and built out the bulk of the code.  I make no promises that this code is amazing.  This was done as a joke, but I hacked together something functional out of it.

The one thing I will say ChatGPT is good for is a sounding board.  Toss code that isn't working at it, ask it to tell you why, when it makes up reasons as to why it's not working and/or gives you back code written in a completely different language, you get so pissed off at it that you realize the error in your code and fix it yourself.

## Usage:
```
Usage: VC2Handler.exe -l <port>

Commands:

Change session:  vc2.sessions
Rename active session:  vc2.rename "new session name"
Save active session to disk:  vc2.save
Close active session:  vc2.close
Quit VisualC2:  vc2.exit

Notifications:

Why did the window flash?  Client connect/disconnect notification
Why is a session green?  The session is new
Why Is a session cyan?  The session has data waiting for you
Why Is a session red?  The session is disconnected
```

As per usual, I think it goes without saying, ***use at your own risk*** I'm not responsible for what you do with it.
