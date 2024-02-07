__________________________________________________________________________________________

Package "Ragdoll Animator"
Version 1.2.3

Made by FImpossible Creations - Filip Moeglich
https://www.FilipMoeglich.pl
FImpossibleCreations@Gmail.com or FImpossibleGames@Gmail.com or Filip.Moeglich@Gmail.com

__________________________________________________________________________________________

Youtube: https://www.youtube.com/channel/UCDvDWSr6MAu1Qy9vX4w8jkw
Facebook: https://www.facebook.com/FImpossibleCreations
Twitter (@FimpossibleC): https://twitter.com/FImpossibleC

___________________________________________________

User Manual:

To use Ragdoll Animator add "Ragdoll Animator" component to your character game object
(Add Component -> Fimpossible Creations -> Ragdoll Animator)
and setup required bone transform references which you will find inside the hierarchy.
Initially Ragdoll Animator tries to find right bones using Humanoid Mecanim Animator if available.
Ragdoll Animator tries to find some bones by names, but IT'S IMPORTANT to check if right ones was selected.

AFTER SETTING UP ALL BONE REFERENCES, open "Ragdoll Generator" tab and switch "Generate Ragdoll".
Now you can check on your scene view how colliders are generated, with sliders you can adjust their size and physical parameters.
After that, you can select "Tweak Ragdoll Colliders" selector and tweak colliders positions/scale without need to go through hierarchy for that.
After all this things done you can hit the Gear ⚙️ button on the top left of Ragdoll Animator inspector window.

There you can setup how ragdoll should behave.
Enter with mouse cursor on the parameters (tooltips are not displayed during playmode)
to read tooltips about which parameter is responsible for.


NEW IN VERSION 1.2.0 (CUSTOM LIMBS CHAINS - For Non-Humanoids or for more customizing Humanoid Ragdoll Setup):
After unfolding "Bones Setup" in the "Setup" bookmark you will notice dropdown next to "Bones Setup" title.
You can switch with it to "Custom Limbs" mode.
Here you can define bone chains for ragdoll setup. Hit "Add Bones Chain" then "Add Next Bone Field"
and pick first bone of some limb, like shoulder / tigh. 
Hit again "Add Next Bone Field" for another bone field, you can click the "▲" button to assign automatically child bone of the bone above but you can still do it manually by drag & dropping bone from hierarchy to the bone field.
Now you may think what is the value with '%' sign, it's percents of total mass of your character (which you define in 'Ragdoll Generator' tab) for this single bone rigidbody which will be generated later.
You can click on the '%' sign to display some helper info.
Next gui element is collider type for the bone ragdoll dummy.
On the most left you see tool button, when you click this, it will display more settings for the single bone.
Here you can adjust few more settings (collider type and bone mass are the same as in the previous view).
With the settings below you can adjust how ragdoll dummy generates, this values are applied to the colliders on the scene
dynamically if you change them, but first you need to open 'Ragdoll Generator' and toggle 'Generate Ragdoll'.
Working with this parameters can be faster to work with, than the manual tweaks with 'Tweak Ragdoll Colliders' 
('Tweak Ragdoll Colliders' changes are discarded every ragdoll generating but the settings set in the tool bone setting are not!)
After setting up all bone chains and generating ragdoll you should be free to run playmode and see your ragdoll working!


! TWEAKING AND FIXING !:

Take in mind that Ragdoll Animator is not yet supporting universal setup for the angle limiting on physical joints,
so if you encounter some of the limbs rotating in wrong ranges you have to adjust it by yourself.

If you want to keep ragdoll when switching scenes, you should enable "Persistent" toggle in Ragdoll Animator setup.
Component is generating secondary skeleton with ragdoll which shouldn't be destroyed to make component work, Persistent is calling "DontDestroyOnLoad" on the generated dummy skeleton.
Instead of using "Persistent" you can assign your object with "Target Parent for Ragdoll Dummy" to generate dummy inside it.
If you want to connect ragdoll dummy physics with character movement, setting "Target Parent for Ragdoll Dummy" helps it.

If some joints are behaving strange, adjusting colliders mass and other rigidbody physical parameters is advised!
IT CAN CHANGE A LOT IN THE JOINTS BEHAVIOUR.

Unity joints are not supporting scaling during playmode, so just initial scale is supported!

If after entering playmode you see there is clearly something wrong with your model + error logs in console,
try assigning "Skeleton Root" manually (bottom of ragdoll animator setup inspector window)

If your character model starts with pelvis in ground, turn off playmode, go to setup, "Corrections" and toggle "Fix Root In Pelvis"
sometimes assigning the "Skeleton Root" is also required to fix it.

If you need full body active ragdoll, you can toggle "Hips Pin" experimental feature.

If you encounter somemething like spine jittery, try lowering configurable spring parameter, increase configurable damp,
try switching "Extended Animator Sync" modes, try different mass on colliders.

Some of the model may not support yet "Chest" bone attachements, then you can just leave "Chest" field empty, generate new ragdoll and check if now character physics behaves correctly.

One time I encountered some error with spine jittery and I couldn't fix it for few hours, 
then I changed few scenes and went back to glitching scene and it was fixed by itself, no idea what changed, Unity mischiefs I guess.

__________________________________________________________________________________________
Description:

Quick Setup Tutorial: https://youtu.be/dC5h-kVR650

Setup your humanoid ragdoll instantly!
Blend ragdolled limbs with animated model!
Enable ragdoll and controll muscle power towards animator pose!

Ragdoll Animator offsers effective and clear solution for 
handling ragdolled humanoid model. (works on generic skeleton too)

Quickly prepare bones to generate colliders, rigidbodies and joints on.
Controll scale/rigidbody mass of all colliders with basic sliders.
Tweak colliders position/scale with additional scene gizmos - without need to finding bones in the hierarchy.

You can animate ragdoll in sync with keyframe animation and provide collision 
detection for arms, head and spine. 
You can smoothly enable free-fall ragdoll mode with possibility to add 
some natural motion to ragdoll with muscle power moving bones towards 
keyframe animator pose with defined power.

Package works on all SRPs! It's not shader related package.

__________________________________________________________________________________________
Changelog:

Version 1.2.3:
- Added full user manual PDF file
- Ragdoll Dummy Post Attach component for non-pre-generated ragdolls (it was in demo package, now it's in core)
- Some cosmetic GUI changes
- Possibility to hide generated dummy in the hierarchy view (it's under "Extra" category)
- Minor changes (Try Animate Pelvis V2 now is making muscles react a bit smoother than in the previous ragdoll animator versions)

Version 1.2.2:
! WARNING ! This version changed some core logics for AnimatePhysics mode and for the TryAnimatePelvis feature.
If something stopped working and you can't solve it, please report it to me via mail.

Version 1.2.1:
- Added 'User_HardSwitchOffRagdollAnimator' method to disable/enable back ragdoll animator if you want to use it only in certain situations
- Added 'User_DestroyRagdollAnimatorAndFreeze' method to destroy ragdoll animator completely and keep lying/death pose on the character
- Added 'User_DestroyRagdollAnimatorAndKeepPhysics' method to destroy ragdoll animator but keeping physical joints on the skeleton (moving them from dumym to the animator skeleton)
- (experimental) Now if you don't generate any ragdoll it will be generated in playmode 
It needs some improvement for better transition when swithching to animator skeleton joints.
- Fixed 'Animate Pelvis' behaviour
- Added interface 'IRagdollAnimatorReceiver' which you can implement to call directly limbs collision enter/exit events with 'Send Collision Events to' feature.
Minor Changes:
- 'Try Animate Pelvis' is turned ON by default
- 'Fix Root in Pelvis' is turned ON by default
- 'Unity Solver Iterations' is = 6 by default

Version 1.2.0.1 (Fix):
- Humanoid Mode with Null Chest Bone Fix
- Some GUI tooltip fixes

Version 1.2.0:
- Now Ragdoll Animator is providing two types of bones setup 'Humanoid Limbs' and 'Custom Limbs'
With custom limbs you get access for more customization for setting up ragdoll for your character/animal/creature.
With 'Custom Limbs' setup you can setup ragdoll for any type of model, like for animals or some strange creatures (gives possibility to ragdoll tails, multiple legs etc.)
You can use 'Custom Limbs' also for humanoid setups (gets automatically converted when switching from humanoid) to have access for more customization settings
like setting type of collider for each of the ragdoll bone.
In the future there may be more customization settings per ragdoll bone.
Custom Limbs can be used also for setting up ragdoll just on few limbs you want, like you can set ragdoll just only for the character's tail if you wish.
- Humanoid support for shoulder bones which can result in more animation matching precision for animated ragdoll
- Example scene with non humanoid "FredCrab" creature HL:Alyx inspired
- Example scene with various creatures with ragdoll setup on
- Hips pin precision adjustements and toggle for V2 hips pin algorithm (Play category -> 'Hips Pin Adjustments' foldout)
- !!! Now foots/fists transforms needs to be assigned manually in the inspector window
- Now 'Blend On Collision' feature will be disabled by default and collider helpers will not be added to the ragdoll limbs if not using 'Blend On Collision' or not using 'Collision Events'
- Possibility to switch off rotation limits for configurable joints when being in non-free-fall mode (to allow rotate bones in any direction demanded by the animator)
- Few optimizations
- Few changes in the ragdoll user methods, like 'quadroped' toggle for detecting facedown/lying on back
- Some quality of life changes in the inspector GUI

Version 1.0.4:
- Now the inspector window contains 3 sections instead of two (setup, play/tweaking, extra)
- Few settings moved from 'Setup' section to the 'Extra' section
- New option to multiply mass of all ragdoll dummy limbs at start (can be really helpful for quickly finding correct mass for your project environment)
- New option to make 'BaseTransform' follow ragdoll automatically on Free Fall - Repose Mode ('Extra' secion)
- New option to disable collision with self dummy ('Extra' secion)
- New option to disable collision with selected colliders ('Extra' secion)
- New option to send each ragdoll dummy limb collision events ('Extra' secion)
- Added few helper methods to the Ragdoll Processor, like:
User_SetPhysicalTorque(), User_LayingOnSide() and others

Version 1.0.3:
- Added "Pre Generate Ragdoll" toggle (visible when generated colliders and setted root bone)
- Active Blending Collisions debug
- Update physics toggle (for animator animate physics update mode)
- Some get-up methods update
- Root bone auto correction try algorithm (so in more cases you don't need to assign root bone manually)
- Added scene gizmos which will help setup characters (visible bones)
- Added logs helping solving common issues with the skeletons
- Added tutorial button

Version 1.0.1:
-Added experimental HipsPin (under "Additional Settings" foldout) option to support full body ragdoll (low animation sync precision)
-Added auto-limbs detection algorithm support for generic rigs, known from Animation Designer 
-Added rigidbody collider symmetrical adjusting tool
-Fixed some ragdoll colliders scene view adjusting logics
-Added "Auto Destroy" ragdoll dummy option
-Added possibility to select parent for dummy ragdoll object
-Cleaner Inspector GUI in Setup View
-Some other minor fixes