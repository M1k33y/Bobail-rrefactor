import { useEffect, useMemo, useState } from "react";

const DEFAULT_FRAME_DURATION = 1200;

export function useRuleDemo(demo) {
  const frames = useMemo(() => demo.frames ?? [], [demo.frames]);
  const [frameIndex, setFrameIndex] = useState(0);
  const [cycle, setCycle] = useState(0);

  useEffect(() => {
    if (frames.length <= 1) {
      return undefined;
    }

    const frame = frames[frameIndex] ?? frames[0];
    const timer = window.setTimeout(() => {
      setFrameIndex((currentIndex) => {
        const nextIndex = currentIndex + 1;

        if (nextIndex >= frames.length) {
          setCycle((value) => value + 1);
          return 0;
        }

        return nextIndex;
      });
    }, frame.duration ?? demo.frameDuration ?? DEFAULT_FRAME_DURATION);

    return () => window.clearTimeout(timer);
  }, [demo.frameDuration, frameIndex, frames]);

  const frame = frames[frameIndex] ?? frames[0] ?? {};
  const game = useMemo(
    () => ({ pieces: frame.pieces ?? [] }),
    [frame.pieces]
  );

  return {
    cycle,
    frame,
    frameIndex,
    game
  };
}
