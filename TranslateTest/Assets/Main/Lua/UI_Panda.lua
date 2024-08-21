UI_Panda={}
local this=UI_Panda;

function this.Show()
	local AssetBundle=CS.UnityEngine.AssetBundle;
	AssetBundle asset=AssetBundle.LoadFromFile(CS.UnityEngine.Application.streamingAssetsPath + "/ui_panda");
	local GameObject=CS.UnityEngine.GameObject;
	GameObject asset = asset:LoadAsset<GameObject>("ui_panda");
end
