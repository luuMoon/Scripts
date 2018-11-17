local M = class("ViewManager")
local pairs = pairs
local Global,HostEvent = Global,HostEvent

function M:ctor()
	self.views = {}
end

function M:FindView(res)
	return self.views[res]
end

function M:AddView(view)
	self.views[view.res] = view
end

function M:RemoveView(view)
	if self.dels then
		self.dels[view.res] = view
	else
		self.views[view.res] = nil
	end
end

function M:ClearDels()
	if self.dels then
		for res,_ in pairs(self.dels) do
			self.views[res] = nil
		end
		self.dels = nil
	end
end

function M:GetView(viewCls)
	local view = self:FindView(viewCls.res)
	if not view then
		view = viewCls.new(self)
	end
	return view
end

function M:HideView(viewCls)
	if not viewCls.res then return end
	local view = self:FindView(viewCls.res)
	if not view then return end
	view:SetVisible(false)
end

function M:ShowView(viewCls)
	if not viewCls.res then return end
	local view = self:FindView(viewCls.res)
	if not view then return end
	view:SetVisible(true)
end

function M:HideAllOtherViews(selfView)
    for _,view in pairs(self.views) do
        if selfView ~= view then
            self:HideView(view)
        end
    end
end

function M:ShowAllOtherViews(selfView)
    for _,view in pairs(self.views) do
        if selfView ~= view then
            self:ShowView(view)
        end
    end
end

function M:OpenView(viewCls)
	if not viewCls.res then
		print("res not assigned")
		return
	end
	local view = self:GetView(viewCls)
	view:Open()
	return view
end

function M:OpenViewWith(viewCls,func,...)
	local view = self:GetView(viewCls)
	func(view,...)
	view:Open()
	return view
end

function M:CloseView(viewCls)
	if not viewCls.res then return end
	local view = self:FindView(viewCls.res)
	if not view then return end
	view:Close()
end

function M:CloseAll()
	self.dels = {}
	for _,view in pairs(self.views) do
		view:Close()
	end
	self:ClearDels()
end

function M:ShowHint(elementName)
	for _, view in pairs(self.views) do
		if view.ShowHint then
			view:ShowHint(elementName)
		end
	end
end

function M:GetHint(elementName)
	for _, view in pairs(self.views) do
		if view.GetHint then
			if view:GetHint(elementName) then
				return true
			end
		end
	end
end

function M:HideHint(elementName)
	for _, view in pairs(self.views) do
		if view.HideHint then
			view:HideHint(elementName)
		end
	end
end

function M:InitHostDeadListener()
	Global:AddEventListener(HostEvent.HOST_DEAD,self.OnHostDead,self)
end

function M:OnHostDead()
	self.dels = {}
	for _, view in pairs(self.views) do
		if view.closeOnHostDead then
			view:Close()
		end
	end
	self:ClearDels()
end

function M:Init()
	self:InitHostDeadListener()
	return self
end

ViewManager = M.new():Init()