local _G = _G
local FsmState = class("FsmState")
function FsmState:GetStateId()
end
function FsmState:Init(params)
end
function FsmState:IsSame(params) return true end
function FsmState:Interruptable()
    return false
end
function FsmState:IsOver()
end
function FsmState:OnEnter()
end
function FsmState:OnUpdate(deltaTime)
end
function FsmState:OnExit()
end

_G.FsmState = FsmState

local StateMachine = class("StateMachine")
function StateMachine:ctor(owner)
    self.stateLookup = {}
    self.owner = owner
end
function StateMachine:Start()
end
function StateMachine:CreateState(stateId)
end
function StateMachine:ChangeState(stateId,forced,...)
    if self.currentState then
        if not forced and self.currentState:GetStateId() == stateId then
            if self.currentState:IsSame(...) then
                return false
            end
        end
        self.currentState:OnExit()
    end
    self.currentState = self:GetState(stateId)
    if self.currentState then
        self.currentState:Init(...)
        self.currentState:OnEnter()
        return true
    end
    return false
end

function StateMachine:OnUpdate(deltaTime)
    if self.currentState then
        self.currentState:OnUpdate(deltaTime)
    end
end
function StateMachine:GetState(stateId)
    local state = self.stateLookup[stateId]
    if not state then
        state = self:CreateState(stateId)
        state.owner = self.owner
        self.stateLookup[stateId] = state
    end
    return state
end

_G.StateMachine = StateMachine