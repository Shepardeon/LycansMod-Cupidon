using Fusion;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cupidon.Unity
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal struct PlayerRefReaderWriter : IElementReaderWriter<PlayerRef>
    {
	    public static IElementReaderWriter<PlayerRef> Instance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe PlayerRef Read(byte* data, int index)
        {
            return *(PlayerRef*)(data + index * 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref PlayerRef ReadRef(byte* data, int index)
        {
            return ref *(PlayerRef*)(data + index * 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(byte* data, int index, PlayerRef val)
        {
            *(PlayerRef*)(data + index * 4) = val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetElementWordCount()
        {
            return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IElementReaderWriter<PlayerRef> GetInstance()
        {
            if (Instance == null)
            {
                Instance = default(PlayerRefReaderWriter);
            }
            return Instance;
        }
    }
}