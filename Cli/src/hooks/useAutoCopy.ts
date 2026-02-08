import { useEffect } from "react"
import { useRenderer } from "@opentui/react"
import type { Selection } from "@opentui/core"

export function useAutoCopy() {
  const renderer = useRenderer()

  useEffect(() => {
    const handleSelection = (selection: Selection) => {
      const text = selection.getSelectedText()
      if (text) {
        renderer.copyToClipboardOSC52(text)
      }
    }

    renderer.on("selection", handleSelection)
    return () => {
      renderer.off("selection", handleSelection)
    }
  }, [renderer])
}
