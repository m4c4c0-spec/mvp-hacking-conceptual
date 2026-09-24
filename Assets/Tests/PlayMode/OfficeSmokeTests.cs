using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Academy.Tests
{
    public sealed class OfficeSmokeTests
    {
        [UnityTest] public IEnumerator OfficeAndUIBootstrapWithoutManualReferences()
        {
            if (Object.FindFirstObjectByType<AcademyBootstrap>() == null)
                new GameObject("Test bootstrap").AddComponent<AcademyBootstrap>();
            yield return null;
            Assert.That(Object.FindFirstObjectByType<AcademyUI>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<DesktopOfficeController>(), Is.Not.Null);
            Assert.That(Object.FindObjectsByType<AcademyInteractable>(FindObjectsSortMode.None).Length, Is.GreaterThanOrEqualTo(5));
            Assert.That(Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Length, Is.GreaterThan(7));
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator DesktopAndXRDispatchTheSameAction()
        {
            var go = new GameObject("Test interactable");
            var target = go.AddComponent<AcademyInteractable>(); target.action = InteractionAction.Laptop;
            int activations = 0;
            target.Activated += action => { Assert.That(action, Is.EqualTo(InteractionAction.Laptop)); activations++; };
            var bridge = go.AddComponent<XRInteractionBridge>(); bridge.target = target;
            target.Activate(); bridge.Activate();
            Assert.That(activations, Is.EqualTo(2));
            Object.Destroy(go); yield return null;
        }
        [UnityTest] public IEnumerator GrabReleaseRestoresObjectPose()
        {
            var go = new GameObject("Notebook"); go.transform.position = new Vector3(1, 2, 3);
            var hand = new GameObject("Hand anchor");
            var target = go.AddComponent<AcademyInteractable>(); target.grabbable = true;
            target.Grab(hand.transform); hand.transform.position = Vector3.one * 5;
            Assert.That(go.transform.parent, Is.EqualTo(hand.transform));
            target.Release(); Assert.That(go.transform.position, Is.EqualTo(new Vector3(1, 2, 3)));
            Assert.That(go.transform.parent, Is.Null);
            Object.Destroy(go); Object.Destroy(hand); yield return null;
        }
        [UnityTest] public IEnumerator HeldObjectRestoresColliderAndFollowsOriginalMovingParentOnReturn()
        {
            var drawer = new GameObject("Test moving drawer");
            var hand = new GameObject("Test hand");
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.SetParent(drawer.transform, false); go.transform.localPosition = new Vector3(0, .2f, 0);
            var target = go.AddComponent<AcademyInteractable>(); target.grabbable = true;
            target.Grab(hand.transform);
            Assert.That(go.GetComponent<Collider>().enabled, Is.False);
            drawer.transform.position = new Vector3(4, 0, 2);
            target.Release();
            Assert.That(go.GetComponent<Collider>().enabled, Is.True);
            Assert.That(go.transform.localPosition, Is.EqualTo(new Vector3(0, .2f, 0)));
            Assert.That(go.transform.parent, Is.EqualTo(drawer.transform));
            Object.Destroy(drawer); Object.Destroy(hand); yield return null;
        }
        [UnityTest] public IEnumerator ClosedArchiveDoorBlocksPlayerAndOpeningAllowsPassage()
        {
            if (Object.FindFirstObjectByType<AcademyBootstrap>() == null)
                new GameObject("Test bootstrap").AddComponent<AcademyBootstrap>();
            yield return null;
            var person = Object.FindFirstObjectByType<DesktopOfficeController>();
            var controller = person.GetComponent<CharacterController>();
            var door = GameObject.Find("Archive door").GetComponent<AcademyInteractable>();
            Vector3 original = person.transform.position;
            controller.enabled = false; person.transform.position = new Vector3(3, .05f, -1.3f); controller.enabled = true;
            Physics.SyncTransforms(); controller.Move(new Vector3(2.5f, 0, 0));
            Assert.That(person.transform.position.x, Is.LessThan(4), "Closed door must stop the capsule.");
            door.Activate(); yield return new WaitForSeconds(1);
            Physics.SyncTransforms(); controller.Move(new Vector3(2.5f, 0, 0));
            Assert.That(person.transform.position.x, Is.GreaterThan(4.4f), "Open doorway must be traversable.");
            controller.enabled = false; person.transform.position = original; controller.enabled = true;
            door.Activate(); yield return new WaitForSeconds(1);
        }
    }
}
