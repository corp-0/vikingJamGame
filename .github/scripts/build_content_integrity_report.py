#!/usr/bin/env python3
from __future__ import annotations

import sys
import xml.etree.ElementTree as ET
from pathlib import Path


TRX_NAMESPACE = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}


def counter_value(counter_node: ET.Element | None, name: str) -> int:
    if counter_node is None:
        return 0

    raw_value = counter_node.attrib.get(name, "0")
    try:
        return int(raw_value)
    except ValueError:
        return 0


def build_report(trx_path: Path) -> list[str]:
    lines = [
        "## Content Integrity Test Report",
        "",
    ]

    if not trx_path.exists():
        lines.extend(
            [
                "- Status: FAILED",
                f"- Details: Could not find TRX file at '{trx_path}'.",
                "",
            ]
        )
        return lines

    try:
        root = ET.parse(trx_path).getroot()
    except Exception as exception:
        lines.extend(
            [
                "- Status: FAILED",
                f"- Details: Could not parse TRX file at '{trx_path}': {exception}",
                "",
            ]
        )
        return lines

    counter = root.find(".//t:ResultSummary/t:Counters", TRX_NAMESPACE)
    total = counter_value(counter, "total")
    passed = counter_value(counter, "passed")
    failed = counter_value(counter, "failed")
    status = "FAILED" if failed > 0 else "PASSED"

    lines.extend(
        [
            f"- Status: {status}",
            f"- Total: {total}",
            f"- Passed: {passed}",
            f"- Failed: {failed}",
            "",
        ]
    )

    if failed == 0:
        return lines

    failed_results = root.findall(".//t:UnitTestResult[@outcome='Failed']", TRX_NAMESPACE)
    for result in failed_results:
        test_name = result.attrib.get("testName", "(unknown test)")
        message_node = result.find("./t:Output/t:ErrorInfo/t:Message", TRX_NAMESPACE)
        stack_node = result.find("./t:Output/t:ErrorInfo/t:StackTrace", TRX_NAMESPACE)

        message = (message_node.text or "").strip() if message_node is not None else "(no error message)"
        stack = (stack_node.text or "").strip() if stack_node is not None else ""

        lines.append(f"### `{test_name}`")
        lines.append("")
        lines.append("```text")
        lines.append(message)
        if stack:
            lines.append("")
            lines.append("Stack trace:")
            lines.append(stack)
        lines.append("```")
        lines.append("")

    return lines


def main() -> int:
    trx_path = Path(sys.argv[1]) if len(sys.argv) > 1 else Path("TestResults/content-integrity.trx")
    report_path = Path(sys.argv[2]) if len(sys.argv) > 2 else Path("content-integrity-report.md")

    report_lines = build_report(trx_path)
    report_path.write_text("\n".join(report_lines), encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
