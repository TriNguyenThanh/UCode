// Code templates cho các ngôn ngữ lập trình

export const codeTemplates: Record<string, string> = {
  cpp: `// C++ Template
#include <iostream>
#include <vector>
#include <string>
#include <algorithm>
using namespace std;

int main() {
    // Viết code của bạn ở đây
    
    return 0;
}`,

  java: `// Java Template
import java.util.*;
import java.io.*;

public class Solution {
    public static void main(String[] args) {
        Scanner sc = new Scanner(System.in);
        
        // Viết code của bạn ở đây
        
        sc.close();
    }
}`,

  python: `# Python Template
def solution():
    # Viết code của bạn ở đây
    pass

if __name__ == "__main__":
    solution()`,

  javascript: `// JavaScript Template
function solution() {
    // Viết code của bạn ở đây
}

// Test
solution();`,

  c: `// C Template
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

int main() {
    // Viết code của bạn ở đây
    
    return 0;
}`,

  csharp: `// C# Template
using System;
using System.Collections.Generic;
using System.Linq;

class Solution {
    static void Main(string[] args) {
        // Viết code của bạn ở đây
    }
}`,

  go: `// Go Template
package main

import (
    "fmt"
)

func main() {
    // Viết code của bạn ở đây
}`,

  rust: `// Rust Template
fn main() {
    // Viết code của bạn ở đây
}`,

  php: `<?php
// PHP Template

// Viết code của bạn ở đây

?>`,

  ruby: `# Ruby Template
def solution
    # Viết code của bạn ở đây
end

# Test
solution()`,

  swift: `// Swift Template
import Foundation

func solution() {
    // Viết code của bạn ở đây
}

// Test
solution()`,

  kotlin: `// Kotlin Template
fun main() {
    // Viết code của bạn ở đây
}`,

  typescript: `// TypeScript Template
function solution(): void {
    // Viết code của bạn ở đây
}

// Test
solution();`,
}

export function getCodeTemplate(language: string): string {
  return codeTemplates[language.toLowerCase()] || `// ${language} code\n\n// Viết code của bạn ở đây\n`
}
