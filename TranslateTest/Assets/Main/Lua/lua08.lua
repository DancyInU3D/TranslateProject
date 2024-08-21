local newObject=CS.UnityEngine.GameObject
local game=newObject.Find('Main Camera')
game.name='hello'
local camera=game:GetComponent("Camera")
camera.fieldOfView=10;
