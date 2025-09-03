// Utilities for parsing Excel and CSV files and generating templates
// Uses dynamic import for 'xlsx' to avoid breaking environments before dependency is installed

export type RowObject = Record<string, any>;

export async function parseExcelFile(file: File): Promise<RowObject[]> {
  const ext = file.name.toLowerCase().split('.').pop();
  if (ext === 'csv') {
    const text = await file.text();
    return parseCsv(text);
  }
  // default to xlsx using dynamic import (typed)
  const XLSX: typeof import('xlsx') = await import('xlsx');
  const data = await file.arrayBuffer();
  const workbook = XLSX.read(data, { type: 'array' });
  const firstSheetName = workbook.SheetNames[0];
  const worksheet = workbook.Sheets[firstSheetName];
  return XLSX.utils.sheet_to_json<RowObject>(worksheet, { defval: '' });
}

export function parseCsv(csvText: string): RowObject[] {
  const lines = csvText.split(/\r?\n/).filter(l => l.trim().length > 0);
  if (lines.length === 0) return [];
  const headers = splitCsvLine(lines[0]);
  const rows: RowObject[] = [];
  for (let i = 1; i < lines.length; i++) {
    const values = splitCsvLine(lines[i]);
    const row: RowObject = {};
    headers.forEach((h, idx) => {
      row[h] = values[idx] ?? '';
    });
    rows.push(row);
  }
  return rows;
}

function splitCsvLine(line: string): string[] {
  const result: string[] = [];
  let current = '';
  let inQuotes = false;
  for (let i = 0; i < line.length; i++) {
    const ch = line[i];
    if (ch === '"') {
      if (inQuotes && line[i + 1] === '"') { // escaped quote
        current += '"';
        i++;
      } else {
        inQuotes = !inQuotes;
      }
    } else if (ch === ',' && !inQuotes) {
      result.push(current);
      current = '';
    } else {
      current += ch;
    }
  }
  result.push(current);
  return result.map(s => s.trim());
}

export async function generateProductsTemplateXlsx(): Promise<Blob> {
  const headers = productTemplateHeaders();
  const sampleRow = productTemplateSample();
  const XLSX = await import(/* webpackChunkName: "xlsx" */ 'xlsx');
  const ws = XLSX.utils.json_to_sheet([sampleRow], { header: headers });
  // Ensure header order and case
  XLSX.utils.sheet_add_aoa(ws, [headers], { origin: 'A1' });
  const wb = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(wb, ws, 'Products');
  const wbout = XLSX.write(wb, { type: 'array', bookType: 'xlsx' });
  return new Blob([wbout], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
}

export function generateProductsTemplateCsv(): Blob {
  const headers = productTemplateHeaders();
  const sample = productTemplateSample();
  const line1 = headers.join(',');
  const line2 = headers.map(h => formatCsvValue(sample[h as keyof typeof sample] as any)).join(',');
  const csv = `${line1}\n${line2}\n`;
  return new Blob([csv], { type: 'text/csv;charset=utf-8;' });
}

function formatCsvValue(val: any): string {
  if (val == null) return '';
  const s = String(val);
  if (s.includes(',') || s.includes('"') || s.includes('\n')) {
    return `"${s.replace(/"/g, '""')}"`;
  }
  return s;
}

function productTemplateHeaders(): string[] {
  return [
    'ProductCode',
    'Name',
    'CategoryType', // e.g., GoldJewelry, Bullion, Coins
    'KaratType', // e.g., 18K, 21K, 22K, 24K
    'Weight',
    'Brand',
    'DesignStyle',
    'SubCategoryId', // optional numeric
    'Shape',
    'PurityCertificateNumber',
    'CountryOfOrigin',
    'YearOfMinting',
    'FaceValue',
    'HasNumismaticValue', // TRUE/FALSE
    'MakingChargesApplicable', // TRUE/FALSE
    'UseProductMakingCharges', // TRUE/FALSE
    'SupplierId' // optional numeric
  ];
}

function productTemplateSample(): Record<string, any> {
  return {
    ProductCode: 'GLD-001',
    Name: 'Classic Bangle',
    CategoryType: 'GoldJewelry',
    KaratType: '22K',
    Weight: 7.5,
    Brand: 'DijaGold',
    DesignStyle: 'Traditional',
    SubCategoryId: '',
    Shape: '65mm',
    PurityCertificateNumber: '',
    CountryOfOrigin: 'EG',
    YearOfMinting: '',
    FaceValue: '',
    HasNumismaticValue: 'FALSE',
    MakingChargesApplicable: 'TRUE',
    UseProductMakingCharges: 'FALSE',
    SupplierId: ''
  };
}
