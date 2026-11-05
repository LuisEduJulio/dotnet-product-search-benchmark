"""Lê os arquivos .trx do dotnet test e grava um resumo em Markdown."""
import glob
import os
import sys
import xml.etree.ElementTree as ET

NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}


def main(results_dir: str, output: str) -> None:
    lines = ["# Testes automatizados", "", "| Suíte | Total | Passaram | Falharam |", "|---|---:|---:|---:|"]
    failures = []
    total = passed = failed = 0

    for path in sorted(glob.glob(os.path.join(results_dir, "*.trx"))):
        root = ET.parse(path).getroot()
        counters = root.find("t:ResultSummary/t:Counters", NS)
        t, p, f = (int(counters.get(k, 0)) for k in ("total", "passed", "failed"))
        total, passed, failed = total + t, passed + p, failed + f
        lines.append(f"| {os.path.splitext(os.path.basename(path))[0]} | {t} | {p} | {f} |")

        for result in root.iterfind("t:Results/t:UnitTestResult", NS):
            if result.get("outcome") == "Failed":
                failures.append(result.get("testName"))

    lines.append(f"| **Total** | **{total}** | **{passed}** | **{failed}** |")
    if failures:
        lines += ["", "## Falharam", ""] + [f"- `{name}`" for name in failures]

    os.makedirs(os.path.dirname(output), exist_ok=True)
    with open(output, "w", encoding="utf-8") as handle:
        handle.write("\n".join(lines) + "\n")
    print("\n".join(lines))


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
