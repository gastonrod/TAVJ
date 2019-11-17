using System;
using Connections;
using Connections.Streams;
using DefaultNamespace;
using UnityEngine;

namespace WorldManagement
{
    public class FramesStorer
    {
        /*
         *  Frames buffer will have the frames I've received
         *  [F0, F1, F2]
         *  Then, it will decide if it needs to send out an interpolated frame when it's asked for one, and do it
         *  on-demand.
         */
        
        private byte[][] _frames;
        private int _bufferSize = 3;
        private byte _charId;
        private bool _mustInterpolate;

        public FramesStorer(byte charId = byte.MaxValue)
        {
            _frames = new byte[_bufferSize][];
            _charId = charId;
        }

        public byte[] GetNextFrame()
        {
            if (_frames[1] == null)
            {
                _mustInterpolate = true;
                return _frames[0];
            }

            byte[] nextFrame;
            if (_mustInterpolate)
            {
                nextFrame = HandleInterpolation();
            }
            else
            {
                nextFrame = _frames[0];
            }
            _mustInterpolate = !_mustInterpolate;
            return nextFrame;
        }

        public bool StoreFrame(byte[] snapshot)
        {
            // First frame ever
            if (_frames[0] == null)
            {
                _frames[0] = snapshot;
                return true;
            }
            // Check if it is an already stored frame
            int i = SnapshotIsPredicted(snapshot);
            if (i != -1)
            {
                StoreFrameWithPredicted(snapshot, i);
                return true;
            }
            // If it's not, then store it
            i = GetLastFrame();
            bool frameIsDiscardable = (Math.Abs(_frames[i][0] - snapshot[0]) < 10 && _frames[i][0] > snapshot[0]);
            if (frameIsDiscardable)
            {
                return false;
            }
            bool frameIsEqual = Utils.FramesAreEqual(snapshot, _frames[i]);
            if (frameIsEqual)
            {
                _frames[i][0] = snapshot[0];
                return true;
            }
            bool bufferIsFull = (i == _frames.Length - 1);
            if (bufferIsFull)
            {
                _frames[i - 1] = snapshot;
            }

            _frames[i+1] = snapshot;
            return true;
        }

        public byte CurrentSnapshotId()
        {
            return _frames[GetLastFrame()][0];
        }


        public void SetCharId(byte charId)
        {
            _charId = charId;
        }

        private int SnapshotIsPredicted(byte[] snapshot)
        {
            byte snapshotId = snapshot[0];
            for (int i = 0; i < _frames.Length; i++)
            {
                if (_frames[i] == null)
                    break;
                if (_frames[i][0] == snapshotId)
                {
                    return i;
                }
            }
            return -1;
        }

        private void StoreFrameWithPredicted(byte[] snapshot, int i)
        {
            byte[] frame = _frames[i];
            for (int j = 1; j < frame.Length;)
            {
                if (frame[j] == _charId)
                {
                    j += UnreliableStream.PACKET_SIZE;
                    continue;
                }
                Buffer.BlockCopy(snapshot, j, frame, j, UnreliableStream.PACKET_SIZE);
                j += UnreliableStream.PACKET_SIZE;
            }
        }

        private int GetLastFrame()
        {
            int lastIdx = _frames.Length - 1;
            for (int i = 1; i < _frames.Length; i++)
            {
                if (_frames[i] == null)
                    return i - 1;
            }
            return _frames[lastIdx] == null ? lastIdx-1 : lastIdx;
        }


        private String FramesToString()
        {
            String s = "Frames: " + _mustInterpolate + "\n";
            for (int i = 0; i < _frames.Length; i++)
            {
                if (_frames[i] == null)
                    return s;
                s += "i: " + Utils.FrameToString(_frames[i]) + "\n";
            }
            return s;
        }
        private byte[] HandleInterpolation()
        {
            byte[] nextFrame = Interpolate(_frames[0], _frames[1]);
            for (int i = 0; i < _frames.Length - 1 && _frames[i] != null; i++)
            {
                _frames[i] = _frames[i + 1];
            }
            _frames[_frames.Length - 1] = null;
            return nextFrame;
        }
        
        private byte[] Interpolate(byte[] frame1, byte[] frame2)
        {
            byte[] interpolatedFrame = new byte[frame2.Length];
            interpolatedFrame[0] = frame2[0];
            for (int j = 1; j < frame2.Length;)
            {
                interpolatedFrame[j] = frame2[j];
                j++;
                interpolatedFrame[j] = frame2[j];
                j++;
                Vector3 lastFramePos    = j > frame1.Length ? Vector3.zero : Utils.ByteArrayToVector3(frame1, j);
                Vector3 newFramePos     = Utils.ByteArrayToVector3(frame2, j);
                Vector3 interpolatedPos = (lastFramePos + newFramePos) / 2;
                Utils.Vector3ToByteArray(interpolatedPos, interpolatedFrame, j);
                j += UnreliableStream.PACKET_SIZE-2;
            }

            return interpolatedFrame;
        }

    }
}