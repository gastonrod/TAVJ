using System;
using Connections;
using Connections.Streams;
using UnityEngine;

namespace Interpolation
{
    public class FramesStorer
    {
        /*
         *  Frames buffer will have x2 the amount of frames I'll store.
         *  Lets say we have F0 (frame 0), F1, and F2
         *  Then the buffer will look like this:
         *  [F0, F0xF1, F1, F1xF2, F2, @]
         */
        
        private byte[][] interpolatedFrames;
        private byte[,] storedFrames;
        private byte[] framesIDs;
        private int bufferSize = 3;
        public FramesStorer()
        {
            interpolatedFrames = new byte[bufferSize*2][];
        }

        public byte[] GetNextFrame()
        {
            byte[] nextFrame = interpolatedFrames[0];
            if (interpolatedFrames[1] == null)
                return nextFrame;
            for (int i = 0; i < interpolatedFrames.Length - 1 && interpolatedFrames[i] != null; i++)
            {
                interpolatedFrames[i] = interpolatedFrames[i + 1];
            }
            interpolatedFrames[interpolatedFrames.Length - 1] = null;
            return nextFrame;
        }
        
        public void StoreFrame(byte[] snapshot, bool bypassDiscard = false)
        {
            byte snapshotID = snapshot[0];
            if (IsDiscardable(snapshotID))
            {
                Debug.Log("Discarding snapshot.");
                return;
            }
            // Frame skipped or not, it's the same.
            int i = GetLastFrame();
            if (i + 1 >= interpolatedFrames.Length)
            {
                // Full buffer, what do I do??
                Debug.Log("Full buffer.");
                return;
            }
            if (i + 2 >= interpolatedFrames.Length)
            {
                interpolatedFrames[i + 1] = snapshot;
                return;
            }
            Interpolate(i, snapshot);
        }

        private void Interpolate(int lastFrameId, byte[] snapshot)
        {
            byte[] lastFrame = interpolatedFrames[lastFrameId];
            if (lastFrame == null)
            {
                interpolatedFrames[lastFrameId] = snapshot;
                return;
            }
            interpolatedFrames[lastFrameId + 2] = snapshot;
            byte[] interpolatedFrame  = new byte[snapshot.Length];
            interpolatedFrame[0] = snapshot[0];
            for (int j = 1; j < snapshot.Length; j++)
            {
                interpolatedFrame[j] = snapshot[j];
                j++;
                interpolatedFrame[j] = snapshot[j];
                j++;
                Vector3 newFramePos = Utils.ByteArrayToVector3(snapshot, j);
                Vector3 lastFramePos = Utils.ByteArrayToVector3(lastFrame, j);
                Vector3 interpolatedPos = (lastFramePos + newFramePos) / 2;
                Utils.Vector3ToByteArray(interpolatedPos, interpolatedFrame, j);
                j += 12;
            }
            interpolatedFrames[lastFrameId + 1] = interpolatedFrame;
        }

        private int GetLastFrame()
        {
            int lastIdx = interpolatedFrames.Length - 1;
            for (int i = 1; i < interpolatedFrames.Length; i++)
            {
                if (interpolatedFrames[i] == null)
                    return i - 1;
            }
            return interpolatedFrames[lastIdx] == null ? -1 : lastIdx;
        }

        private bool IsDiscardable(byte snapshotId)
        {
            int i = GetLastFrame();
            if (i == 0)
                return false;
            byte currentSnapshotId = interpolatedFrames[i][0];
            return snapshotId <= currentSnapshotId && Math.Abs(snapshotId - currentSnapshotId) < 10;
        }

        public byte CurrentSnapshotID()
        {
            return interpolatedFrames[GetLastFrame()][0];
        }
    }
}