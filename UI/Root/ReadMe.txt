## UI基础类

* ViewManager  Lua管理View类
	
	1.添加/删除view
	2.获取view
	3.隐藏/显示view
	4.监听死亡等事件，管理view

* UiView LuaView基础类

	1.Open/Close，开关，加载View资源
	2.GetSlibingIndex,SetSlibingIndex, 获取/设置显示层级
	3.UiViewSequence管理view层级顺序,分为2个UICamra,top为上层

* ViewLocator 加载View

* DlgView c#View基础类

	1.使用AssetModelFactory加载资源panel
	2.DisplayAsLast,DisplayAsFirst,SetSlibingIndex,GetSlibingIndex设置层级
	3.设置injector,导入相关引用(button,image,text,component....)

* LuaInjector  导入引用供lua端使用
