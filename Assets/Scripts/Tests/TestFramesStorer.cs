using DefaultNamespace;
using WorldManagement;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class TestFramesStorer
    {
        private static int PACKET_SIZE = 14;
        Vector3 firstPosition = new Vector3(1,1,1);
        Vector3 interpolatedPosition = new Vector3(1.5f, 1.5f, 1.5f);
        Vector3 secondPosition = new Vector3(2,2,2);
        
        [Test]
        public void TestFirstFrameInjection()
        {
            byte[] frame_1 = GetFrame(new []{firstPosition}, 0);
            FramesStorer fS = new FramesStorer();
            fS.StoreFrame(frame_1);
            Assert.That(fS.GetNextFrame(), Is.EquivalentTo(frame_1));
        }

        [Test]
        public void TestInterpolationTwoFrames()
        {
            FramesStorer fS = new FramesStorer();
            byte[] frame_1 = GetFrame(new[] {firstPosition}, 0);

            byte[] interpolated_frame = GetFrame(new[] {interpolatedPosition}, 1);
            
            byte[] frame_2 = GetFrame(new[] {secondPosition}, 1);
            
            fS.StoreFrame(frame_1);
            fS.StoreFrame(frame_2);
            Assert.That(fS.GetNextFrame(), Is.EquivalentTo(frame_1));
            Assert.That(fS.GetNextFrame(), Is.EquivalentTo(interpolated_frame));
            Assert.That(fS.GetNextFrame(), Is.EquivalentTo(frame_2));
        }

        [Test]
        public void TestFrameDrop()
        {
            FramesStorer fS = new FramesStorer();
            Vector3[] firstPositionV = {firstPosition};
            Vector3[] interpolatedPositionV = {interpolatedPosition};
            Vector3[] secondPositionV = {secondPosition};
            Vector3[] thirdPositionV = {new Vector3(3,3,3)};
            fS.StoreFrame(GetFrame(firstPositionV, 0));
            fS.StoreFrame(GetFrame(interpolatedPositionV, 1));
            
            Assert.IsTrue(fS.StoreFrame(GetFrame(secondPositionV, 2)));
            Assert.IsFalse(fS.StoreFrame(GetFrame(thirdPositionV, 3)));
        }

        [Test]
        public void TestCurrentSnapshotId()
        {
            FramesStorer fS = new FramesStorer();

            byte frameIdx = 0;
            Vector3[] firstPositionV = {firstPosition};
            Vector3[] interpolatedPositionV = {interpolatedPosition};
            Vector3[] secondPositionV = {secondPosition};
            Vector3[] thirdPositionV = {new Vector3(3,3,3)};
            fS.StoreFrame(GetFrame(firstPositionV, frameIdx));
            Assert.That(fS.CurrentSnapshotId(), Is.EqualTo(frameIdx));
            frameIdx++;

            fS.StoreFrame(GetFrame(interpolatedPositionV, frameIdx));
            Assert.That(fS.CurrentSnapshotId(), Is.EqualTo(frameIdx));
            frameIdx++;

            fS.StoreFrame(GetFrame(secondPositionV, frameIdx));
            Assert.That(fS.CurrentSnapshotId(), Is.EqualTo(frameIdx));
            
            fS.StoreFrame(GetFrame(thirdPositionV, (byte)(frameIdx+1)));
            Assert.That(fS.CurrentSnapshotId(), Is.EqualTo(frameIdx));
        }

        [Test]
        public void TestPredictionInjection()
        {
            FramesStorer fS = new FramesStorer( 0);
            byte[] frame1 = GetFrame(new []{firstPosition, firstPosition}, 0);
            fS.StoreFrame(frame1);
            
            Vector3 frame2Pos0 = new Vector3(2, 1, 1);
            byte[] predictedFrame = GetFrame(new[] {frame2Pos0, firstPosition}, 1);
            fS.StoreFrame(predictedFrame);
            
            byte[] receivedFrame = GetFrame(new[] {firstPosition, secondPosition}, 1);
            fS.StoreFrame(receivedFrame);
            
            /*
             *  Frames should look like this:
             *  F1: {(1,1,1), (1,1,1)}
             *  F1': {(1.5,1,1), (1.5,1.5,1.5)}
             *  F2: {(2,1,1), (2,2,2)}
             */
            Vector3 interpolatedPos0 = new Vector3(1.5f, 1, 1);
            byte[] interpolatedFrame = GetFrame(new[] {interpolatedPos0, interpolatedPosition}, 1);
            byte[] frame2 = GetFrame(new[] {frame2Pos0, secondPosition}, 1);

            
            Assert.That(fS.GetNextFrame(), Is.EquivalentTo(frame1));
            Assert.That(fS.GetNextFrame(), Is.EquivalentTo(interpolatedFrame));
            Assert.That(fS.GetNextFrame(), Is.EquivalentTo(frame2));
        }
        [Test]
        public void TestStoreAndReceive()
        {
            FramesStorer fS = new FramesStorer();
            byte[] frame1 = GetFrame(new[] {firstPosition}, 0);
            byte[] frame2 = GetFrame(new[] {secondPosition}, 1);
            Vector3 thirdPosition = new Vector3(3,3,3);
            Vector3 secondInterpolation = new Vector3(2.5f, 2.5f, 2.5f);
            byte[] interp2 = GetFrame(new[] {secondInterpolation}, 2);
            byte[] frame3 = GetFrame(new[] {new Vector3(3,3,3)}, 2);
            fS.StoreFrame(frame1);
            fS.StoreFrame(frame2);
            // {frame1, frame2}
            fS.GetNextFrame();
            fS.GetNextFrame();
            fS.GetNextFrame();
            fS.StoreFrame(frame3);
            Assert.That(fS.GetNextFrame(), Is.EquivalentTo(interp2));
            Assert.That(fS.GetNextFrame(), Is.EquivalentTo(frame3));
        }

        private byte[] GetFrame(Vector3[] positions, byte frameId)
        {
            byte[] frame = new byte[PACKET_SIZE * positions.Length + 1];
            frame[0] = frameId;
            for (int i = 0; i < positions.Length; i++)
            {
                setPosition(positions[i], frame, 1+PACKET_SIZE*i, (byte)i);
            }

            return frame;
        }
        
        private void setPosition(Vector3 pos, byte[] buffer, int idx, byte id)
        {
            buffer[idx++] = id; // Object ID
            buffer[idx++] = 0; // Object Primitive Type
            Utils.Vector3ToByteArray(pos, buffer, idx);
        }
    }
}