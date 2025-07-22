#!/bin/bash

# Define o arquivo de saída
OUTPUT_FILE="exported_structure_and_content.txt"

# Remove o arquivo de saída se já existir
rm -f "$OUTPUT_FILE"

# Função para exportar a estrutura e o conteúdo
export_structure_and_content() {
  local dir_path="$1"

  # Adiciona a árvore de diretórios ao arquivo de saída, excluindo bin e obj
  echo "Estrutura de diretórios de $dir_path:" >> "$OUTPUT_FILE"
  find "$dir_path" -type d \( -name bin -o -name obj \) -prune -o -print >> "$OUTPUT_FILE"

  # Adiciona o conteúdo dos arquivos ao arquivo de saída
  echo -e "\nConteúdo dos arquivos em $dir_path:" >> "$OUTPUT_FILE"
  find "$dir_path" -type d \( -name bin -o -name obj \) -prune -o -type f -print | while read -r file; do
    echo -e "\n--- $file ---" >> "$OUTPUT_FILE"
    cat "$file" >> "$OUTPUT_FILE" 2>/dev/null
  done
}

# Exporta a estrutura e o conteúdo dos dois projetos
export_structure_and_content "FlightServiceApi"
export_structure_and_content "FlightServiceClientApp"

echo "Exportação concluída. O resultado está em $OUTPUT_FILE."
