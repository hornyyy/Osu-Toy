# Osu!Toy

*Pleasure is Just a Tap Away - Zak*

This is a custom build of osu!lazer that includes a custom mod for Buttplug.io support!

To use, make sure intiface is running and you have a device connected, then enable the `Toy` mod in mods menu, `and your motor is revved and VROOOM VROOOM AWOOOOOOGAHHH HUFFA HUFFA WAOOOOOOOOO`

## Settings

If you are running Intiface on a different port or want to use a different address entirely, you can change the address in Settings under Toy > Intiface. If the game is already connected to an existing Intiface, you will need to restart for the change to take effect.

## Mod Config

This mod has a few config settings, and a few of them aren't super clear on what they do, so I'll document them here:

1. `Motor Speed Max` - The max speed all motors will be limited to. As you might expect, 1=100%, 0=0%.
2. `Combo Factor Max` - Sets the percentage of the total notes that will be used as the basis for full motor power on all combo motors. For example, a value of 0.3, the default, will result in full motor power when 30% of the max notes in the song are in a combo. This is so that you can modulate the power based on how good you are at osu and what level of combo you can actually sustain.
3. `Motor N Behavior` - Sets the motor behavior to one of the below modes:
    * `Do Nothing` - This mode means that no data will be sent to that motor.
    * `Bind to Health` - The higher the health, the faster the motor. This is on an x^4 curve because I thought that would be fun.
         * `Invert` - The lower the health, the faster the motor. Makes failing fun!! 
    * `Bind to Combo` - The higher the combo, the faster the motor, until you hit the limit created by the `Combo Factor Max` and the current song.
         * `Invert` - The lower the combo, the faster the motor. Makes failing fun, in a more differenter way.
    * `Bind to Accuracy` -   The higher the accuracy, the faster the motor.
         * `Invert` - The lower the acc, the faster the motor. Makes failing fun, in an even more differenter way.
    * `Bind to Hit` - Every time you hit a note, it sends a pulse to the toy that lasts for a short time. **This is really WIP so don't use yet**, and it might be really bad because the Bluetooth latency might be really noticeable
         * `Invert` - Vibrates on misses instead of hits. Makes failing fun, in an even super more differenter way.
    * I was really dumb with the names of these, Motor 1 Behavior corresponds to motor ID 0. I'm tired rn and will fix it later, fuck you.
   
(I tried to put these in the game, but while the settings entries have a description option, the game doesn't seem to do anything with that for some stupid reason?)

## Shortcomings

Multi-device support is not good rn. Basically, it will run all connected devices, but the `Motor 0`s of each device will all be on the same setting. The good thing is that calling a motor beyond what a device is setup for has no consequences, so it's fine if multiple devices have different numbers of motors, but like I said they can't be configured independently.

I wrote this readme very late at night so if this doesn't make sense yell at me and I'll re-write it later (or you can!)