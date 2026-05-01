using UnityEngine;

namespace TW.ActionStates
{
	public class ActionStateInvoker : MonoBehaviour
	{
		private ActionStateController controller;

		public void SetControllerByComponent(Component component)
		{

			component?.TryGetComponent(out controller);
		}

		public void SetControllerByGameObject(GameObject component)
		{
			component.TryGetComponent(out controller);
		}

		public void SetState(ActionState state)
		{
			if (!controller)
				return;

			controller.State = state;
		}

		public void SetConditionedDefault()
		{
			if (!controller)
				return;

			controller.SetConditionedDefault();
		}
	}

}
