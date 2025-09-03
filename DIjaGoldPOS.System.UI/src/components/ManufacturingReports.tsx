import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from './ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from './ui/tabs';
import api from '../services/api';

const ManufacturingReports: React.FC = () => {
  const [tab, setTab] = React.useState<string>('dashboard');
  const [startDate, setStartDate] = React.useState<string>('');
  const [endDate, setEndDate] = React.useState<string>('');
  const [branchId, setBranchId] = React.useState<number | ''>('');
  const [technicianId, setTechnicianId] = React.useState<number | ''>('');

  const [branches, setBranches] = React.useState<Array<{ id: number; name: string }>>([]);
  const [technicians, setTechnicians] = React.useState<Array<{ id: number; fullName: string }>>([]);

  const [loading, setLoading] = React.useState<boolean>(false);
  const [error, setError] = React.useState<string | null>(null);

  const [dashboard, setDashboard] = React.useState<any>(null);
  const [rawGold, setRawGold] = React.useState<any>(null);
  const [efficiency, setEfficiency] = React.useState<any>(null);
  const [cost, setCost] = React.useState<any>(null);
  const [workflow, setWorkflow] = React.useState<any>(null);

  const sd = startDate || undefined;
  const ed = endDate || undefined;
  const bId = branchId === '' ? undefined : Number(branchId);
  const tId = technicianId === '' ? undefined : Number(technicianId);

  // Load branches once
  React.useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const data = await api.branches.getBranches({ pageNumber: 1, pageSize: 200 });
        if (!cancelled) {
          const items = (data.items || []).map((b: any) => ({ id: b.id, name: b.name }));
          setBranches(items);
        }
      } catch {
        // ignore silently for filters
      }
    })();
    return () => { cancelled = true; };
  }, []);

  // Load technicians when branch changes
  React.useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const techs = await api.technicians.getActiveTechnicians(bId);
        if (!cancelled) {
          setTechnicians(techs.map((t: any) => ({ id: t.id, fullName: t.fullName })));
        }
      } catch {
        // ignore silently for filters
      }
    })();
    return () => { cancelled = true; };
  }, [bId]);

  // Simple CSV export utility
  function exportToCsv(filename: string, headers: string[], rows: any[]) {
    const escape = (val: any) => {
      if (val === null || val === undefined) return '';
      const s = String(val).replace(/"/g, '""');
      if (/[",\n]/.test(s)) return `"${s}"`;
      return s;
    };
    const csv = [headers.join(',')]
      .concat(rows.map(r => headers.map(h => escape(r[h])).join(',')))
      .join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', filename);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  }

  React.useEffect(() => {
    let cancelled = false;
    async function load() {
      setLoading(true);
      setError(null);
      try {
        if (tab === 'dashboard') {
          const data = await api.manufacturingReports.getManufacturingDashboard(sd, ed, bId, tId);
          if (!cancelled) setDashboard(data);
        } else if (tab === 'raw-gold') {
          const data = await api.manufacturingReports.getRawGoldUtilizationReport(sd, ed, bId, tId);
          if (!cancelled) setRawGold(data);
        } else if (tab === 'efficiency') {
          const data = await api.manufacturingReports.getEfficiencyReport(sd, ed, bId, tId);
          if (!cancelled) setEfficiency(data);
        } else if (tab === 'cost') {
          const data = await api.manufacturingReports.getCostAnalysisReport(sd, ed, bId, tId);
          if (!cancelled) setCost(data);
        } else if (tab === 'workflow') {
          const data = await api.manufacturingReports.getWorkflowPerformanceReport(sd, ed, bId, tId);
          if (!cancelled) setWorkflow(data);
        }
      } catch (e: any) {
        if (!cancelled) setError(e?.message || 'Failed to load report');
      } finally {
        if (!cancelled) setLoading(false);
      }
    }
    load();
    return () => {
      cancelled = true;
    };
  }, [tab, sd, ed, bId, tId]);

  return (
    <div className="space-y-6">
      <div className="flex items-end justify-between gap-4 flex-wrap">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Manufacturing Reports</h1>
          <p className="text-muted-foreground">Rebuilt, modular reports powered by backend analytics.</p>
        </div>

        <div className="flex items-center gap-3 flex-wrap">
          <div className="flex items-center gap-2">
            <label className="text-sm text-muted-foreground">From</label>
            <input
              type="date"
              className="h-9 rounded-md border px-2 text-sm"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
            />
          </div>
          <div className="flex items-center gap-2">
            <label className="text-sm text-muted-foreground">To</label>
            <input
              type="date"
              className="h-9 rounded-md border px-2 text-sm"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
            />
          </div>
          <div className="flex items-center gap-2">
            <label className="text-sm text-muted-foreground">Branch</label>
            <select
              className="h-9 rounded-md border px-2 text-sm min-w-[160px]"
              value={branchId}
              onChange={(e) => setBranchId(e.target.value ? Number(e.target.value) : '')}
            >
              <option value="">All</option>
              {branches.map((b) => (
                <option key={b.id} value={b.id}>{b.name}</option>
              ))}
            </select>
          </div>
          <div className="flex items-center gap-2">
            <label className="text-sm text-muted-foreground">Technician</label>
            <select
              className="h-9 rounded-md border px-2 text-sm min-w-[160px]"
              value={technicianId}
              onChange={(e) => setTechnicianId(e.target.value ? Number(e.target.value) : '')}
            >
              <option value="">All</option>
              {technicians.map((t) => (
                <option key={t.id} value={t.id}>{t.fullName}</option>
              ))}
            </select>
          </div>
        </div>
      </div>

      <Tabs value={tab} onValueChange={setTab} className="w-full">
        <TabsList>
          <TabsTrigger value="dashboard">Dashboard</TabsTrigger>
          <TabsTrigger value="raw-gold">Raw Gold</TabsTrigger>
          <TabsTrigger value="efficiency">Efficiency</TabsTrigger>
          <TabsTrigger value="cost">Cost Analysis</TabsTrigger>
          <TabsTrigger value="workflow">Workflow</TabsTrigger>
        </TabsList>

        <TabsContent value="dashboard">
          <Card>
            <CardHeader>
              <CardTitle>Dashboard Summary</CardTitle>
            </CardHeader>
            <CardContent>
              {loading && <p className="text-sm text-muted-foreground">Loading...</p>}
              {error && <p className="text-sm text-red-600">{error}</p>}
              {!loading && !error && (
                dashboard ? (
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                    <div className="rounded border p-3">
                      <div className="text-xs text-muted-foreground">Raw Gold Purchased</div>
                      <div className="text-xl font-semibold">{dashboard?.Summary?.TotalRawGoldPurchased ?? 0}</div>
                    </div>
                    <div className="rounded border p-3">
                      <div className="text-xs text-muted-foreground">Raw Gold Consumed</div>
                      <div className="text-xl font-semibold">{dashboard?.Summary?.TotalRawGoldConsumed ?? 0}</div>
                    </div>
                    <div className="rounded border p-3">
                      <div className="text-xs text-muted-foreground">Utilization Rate</div>
                      <div className="text-xl font-semibold">{(dashboard?.Summary?.RawGoldUtilizationRate ?? 0).toFixed(2)}%</div>
                    </div>
                    <div className="rounded border p-3">
                      <div className="text-xs text-muted-foreground">Products Manufactured</div>
                      <div className="text-xl font-semibold">{dashboard?.Summary?.TotalProductsManufactured ?? 0}</div>
                    </div>
                    <div className="rounded border p-3">
                      <div className="text-xs text-muted-foreground">Overall Completion</div>
                      <div className="text-xl font-semibold">{(dashboard?.Summary?.OverallCompletionRate ?? 0).toFixed(2)}%</div>
                    </div>
                    <div className="rounded border p-3">
                      <div className="text-xs text-muted-foreground">Total Manufacturing Cost</div>
                      <div className="text-xl font-semibold">{dashboard?.Summary?.TotalManufacturingCost ?? 0}</div>
                    </div>
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">
                    No data for the selected period. Filters: {startDate || '—'} to {endDate || '—'}.
                  </p>
                )
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="raw-gold">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Raw Gold Utilization</CardTitle>
                {rawGold && (
                  <div className="flex gap-2">
                    {Array.isArray(rawGold.BySupplier) && rawGold.BySupplier.length > 0 && (
                      <button
                        className="h-8 px-2 border rounded text-xs"
                        onClick={() => {
                          const headers = ['SupplierName','TotalPurchased','TotalConsumed','UtilizationRate'];
                          exportToCsv('raw-gold-by-supplier.csv', headers, rawGold.BySupplier);
                        }}
                      >Export Suppliers</button>
                    )}
                    {Array.isArray(rawGold.ByKaratType) && rawGold.ByKaratType.length > 0 && (
                      <button
                        className="h-8 px-2 border rounded text-xs"
                        onClick={() => {
                          const headers = ['KaratTypeName','TotalPurchased','TotalConsumed','UtilizationRate'];
                          exportToCsv('raw-gold-by-karat.csv', headers, rawGold.ByKaratType);
                        }}
                      >Export Karats</button>
                    )}
                    {Array.isArray(rawGold.ByProductType) && rawGold.ByProductType.length > 0 && (
                      <button
                        className="h-8 px-2 border rounded text-xs"
                        onClick={() => {
                          const headers = ['ProductTypeName','TotalPurchased','TotalConsumed','UtilizationRate'];
                          exportToCsv('raw-gold-by-product-type.csv', headers, rawGold.ByProductType);
                        }}
                      >Export Product Types</button>
                    )}
                  </div>
                )}
              </div>
            </CardHeader>
            <CardContent>
              {loading && <p className="text-sm text-muted-foreground">Loading...</p>}
              {error && <p className="text-sm text-red-600">{error}</p>}
              {!loading && !error && (
                rawGold ? (
                  <div className="space-y-4 text-sm">
                    <div className="space-y-2">
                      <div>Total Purchased: {rawGold.TotalRawGoldPurchased}</div>
                      <div>Total Consumed: {rawGold.TotalRawGoldConsumed}</div>
                      <div>Utilization Rate: {(rawGold.RawGoldUtilizationRate ?? 0).toFixed(2)}%</div>
                    </div>

                    {Array.isArray(rawGold.BySupplier) && rawGold.BySupplier.length > 0 && (
                      <div>
                        <div className="font-medium mb-2">By Supplier</div>
                        <div className="overflow-x-auto">
                          <table className="w-full text-left border text-xs">
                            <thead className="bg-muted/50">
                              <tr>
                                <th className="p-2 border">Supplier</th>
                                <th className="p-2 border">Purchased</th>
                                <th className="p-2 border">Consumed</th>
                                <th className="p-2 border">Utilization %</th>
                              </tr>
                            </thead>
                            <tbody>
                              {rawGold.BySupplier.map((row: any, idx: number) => (
                                <tr key={idx}>
                                  <td className="p-2 border">{row.SupplierName}</td>
                                  <td className="p-2 border">{row.TotalPurchased}</td>
                                  <td className="p-2 border">{row.TotalConsumed}</td>
                                  <td className="p-2 border">{(row.UtilizationRate ?? 0).toFixed(2)}%</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      </div>
                    )}

                    {Array.isArray(rawGold.ByKaratType) && rawGold.ByKaratType.length > 0 && (
                      <div>
                        <div className="font-medium mb-2">By Karat Type</div>
                        <div className="overflow-x-auto">
                          <table className="w-full text-left border text-xs">
                            <thead className="bg-muted/50">
                              <tr>
                                <th className="p-2 border">Karat</th>
                                <th className="p-2 border">Purchased</th>
                                <th className="p-2 border">Consumed</th>
                                <th className="p-2 border">Utilization %</th>
                              </tr>
                            </thead>
                            <tbody>
                              {rawGold.ByKaratType.map((row: any, idx: number) => (
                                <tr key={idx}>
                                  <td className="p-2 border">{row.KaratTypeName}</td>
                                  <td className="p-2 border">{row.TotalPurchased}</td>
                                  <td className="p-2 border">{row.TotalConsumed}</td>
                                  <td className="p-2 border">{(row.UtilizationRate ?? 0).toFixed(2)}%</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      </div>
                    )}

                    {Array.isArray(rawGold.ByProductType) && rawGold.ByProductType.length > 0 && (
                      <div>
                        <div className="font-medium mb-2">By Product Type</div>
                        <div className="overflow-x-auto">
                          <table className="w-full text-left border text-xs">
                            <thead className="bg-muted/50">
                              <tr>
                                <th className="p-2 border">Product Type</th>
                                <th className="p-2 border">Purchased</th>
                                <th className="p-2 border">Consumed</th>
                                <th className="p-2 border">Utilization %</th>
                              </tr>
                            </thead>
                            <tbody>
                              {rawGold.ByProductType.map((row: any, idx: number) => (
                                <tr key={idx}>
                                  <td className="p-2 border">{row.ProductTypeName}</td>
                                  <td className="p-2 border">{row.TotalPurchased}</td>
                                  <td className="p-2 border">{row.TotalConsumed}</td>
                                  <td className="p-2 border">{(row.UtilizationRate ?? 0).toFixed(2)}%</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      </div>
                    )}
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">No data for selected period.</p>
                )
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="efficiency">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Efficiency Report</CardTitle>
                {efficiency && (
                  <div className="flex gap-2">
                    {Array.isArray(efficiency.ByTechnician) && efficiency.ByTechnician.length > 0 && (
                      <button
                        className="h-8 px-2 border rounded text-xs"
                        onClick={() => {
                          const headers = ['TechnicianName','TotalRecords','CompletedRecords','CompletionRate'];
                          exportToCsv('efficiency-by-technician.csv', headers, efficiency.ByTechnician);
                        }}
                      >Export Technicians</button>
                    )}
                    {Array.isArray(efficiency.ByBranch) && efficiency.ByBranch.length > 0 && (
                      <button
                        className="h-8 px-2 border rounded text-xs"
                        onClick={() => {
                          const headers = ['BranchName','TotalRecords','CompletedRecords','CompletionRate'];
                          exportToCsv('efficiency-by-branch.csv', headers, efficiency.ByBranch);
                        }}
                      >Export Branches</button>
                    )}
                    {Array.isArray(efficiency.EfficiencyTrend) && efficiency.EfficiencyTrend.length > 0 && (
                      <button
                        className="h-8 px-2 border rounded text-xs"
                        onClick={() => {
                          const headers = ['Month','CompletionRate','AverageEfficiency'];
                          exportToCsv('efficiency-trend.csv', headers, efficiency.EfficiencyTrend);
                        }}
                      >Export Trend</button>
                    )}
                  </div>
                )}
              </div>
            </CardHeader>
            <CardContent>
              {loading && <p className="text-sm text-muted-foreground">Loading...</p>}
              {error && <p className="text-sm text-red-600">{error}</p>}
              {!loading && !error && (
                efficiency ? (
                  <div className="space-y-4 text-sm">
                    <div className="space-y-2">
                      <div>Total Records: {efficiency.TotalManufacturingRecords}</div>
                      <div>Completed: {efficiency.CompletedRecords}</div>
                      <div>Completion Rate: {(efficiency.OverallCompletionRate ?? 0).toFixed(2)}%</div>
                      <div>Avg Efficiency Rating: {(efficiency.AverageEfficiencyRating ?? 0).toFixed(2)}</div>
                    </div>

                    {Array.isArray(efficiency.ByTechnician) && efficiency.ByTechnician.length > 0 && (
                      <div>
                        <div className="font-medium mb-2">By Technician</div>
                        <div className="overflow-x-auto">
                          <table className="w-full text-left border text-xs">
                            <thead className="bg-muted/50">
                              <tr>
                                <th className="p-2 border">Technician</th>
                                <th className="p-2 border">Total</th>
                                <th className="p-2 border">Completed</th>
                                <th className="p-2 border">Completion %</th>
                              </tr>
                            </thead>
                            <tbody>
                              {efficiency.ByTechnician.map((row: any, idx: number) => (
                                <tr key={idx}>
                                  <td className="p-2 border">{row.TechnicianName}</td>
                                  <td className="p-2 border">{row.TotalRecords}</td>
                                  <td className="p-2 border">{row.CompletedRecords}</td>
                                  <td className="p-2 border">{(row.CompletionRate ?? 0).toFixed(2)}%</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      </div>
                    )}

                    {Array.isArray(efficiency.ByBranch) && efficiency.ByBranch.length > 0 && (
                      <div>
                        <div className="font-medium mb-2">By Branch</div>
                        <div className="overflow-x-auto">
                          <table className="w-full text-left border text-xs">
                            <thead className="bg-muted/50">
                              <tr>
                                <th className="p-2 border">Branch</th>
                                <th className="p-2 border">Total</th>
                                <th className="p-2 border">Completed</th>
                                <th className="p-2 border">Completion %</th>
                              </tr>
                            </thead>
                            <tbody>
                              {efficiency.ByBranch.map((row: any, idx: number) => (
                                <tr key={idx}>
                                  <td className="p-2 border">{row.BranchName}</td>
                                  <td className="p-2 border">{row.TotalRecords}</td>
                                  <td className="p-2 border">{row.CompletedRecords}</td>
                                  <td className="p-2 border">{(row.CompletionRate ?? 0).toFixed(2)}%</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      </div>
                    )}

                    {Array.isArray(efficiency.EfficiencyTrend) && efficiency.EfficiencyTrend.length > 0 && (
                      <div>
                        <div className="font-medium mb-2">Efficiency Trend</div>
                        <div className="overflow-x-auto">
                          <table className="w-full text-left border text-xs">
                            <thead className="bg-muted/50">
                              <tr>
                                <th className="p-2 border">Month</th>
                                <th className="p-2 border">Completion %</th>
                                <th className="p-2 border">Avg Efficiency</th>
                              </tr>
                            </thead>
                            <tbody>
                              {efficiency.EfficiencyTrend.map((row: any, idx: number) => (
                                <tr key={idx}>
                                  <td className="p-2 border">{row.Month}</td>
                                  <td className="p-2 border">{(row.CompletionRate ?? 0).toFixed(2)}%</td>
                                  <td className="p-2 border">{(row.AverageEfficiency ?? 0).toFixed(2)}</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      </div>
                    )}
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">No data for selected period.</p>
                )
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="cost">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Cost Analysis</CardTitle>
                {cost && (
                  <div className="flex gap-2">
                    {Array.isArray(cost.CostBreakdownByProductType) && cost.CostBreakdownByProductType.length > 0 && (
                      <button
                        className="h-8 px-2 border rounded text-xs"
                        onClick={() => {
                          const headers = ['ProductTypeName','RawGoldCost','ManufacturingCost','TotalCost'];
                          exportToCsv('cost-by-product-type.csv', headers, cost.CostBreakdownByProductType);
                        }}
                      >Export Breakdown</button>
                    )}
                    {Array.isArray(cost.CostTrend) && cost.CostTrend.length > 0 && (
                      <button
                        className="h-8 px-2 border rounded text-xs"
                        onClick={() => {
                          const headers = ['Month','TotalCost','AverageCostPerGram'];
                          exportToCsv('cost-trend.csv', headers, cost.CostTrend);
                        }}
                      >Export Trend</button>
                    )}
                    {Array.isArray(cost.TopCostProducts) && cost.TopCostProducts.length > 0 && (
                      <button
                        className="h-8 px-2 border rounded text-xs"
                        onClick={() => {
                          const headers = ['ProductName','TotalCost'];
                          exportToCsv('top-cost-products.csv', headers, cost.TopCostProducts);
                        }}
                      >Export Top Products</button>
                    )}
                  </div>
                )}
              </div>
            </CardHeader>
            <CardContent>
              {loading && <p className="text-sm text-muted-foreground">Loading...</p>}
              {error && <p className="text-sm text-red-600">{error}</p>}
              {!loading && !error && (
                cost ? (
                  <div className="space-y-4 text-sm">
                    <div className="space-y-2">
                      <div>Total Raw Gold Cost: {cost.TotalRawGoldCost}</div>
                      <div>Total Manufacturing Cost: {cost.TotalManufacturingCost}</div>
                      <div>Average Cost/Gram: {(cost.AverageCostPerGram ?? 0).toFixed(2)}</div>
                    </div>

                    {Array.isArray(cost.CostBreakdownByProductType) && cost.CostBreakdownByProductType.length > 0 && (
                      <div>
                        <div className="font-medium mb-2">Cost Breakdown by Product Type</div>
                        <div className="overflow-x-auto">
                          <table className="w-full text-left border text-xs">
                            <thead className="bg-muted/50">
                              <tr>
                                <th className="p-2 border">Product Type</th>
                                <th className="p-2 border">Raw Gold Cost</th>
                                <th className="p-2 border">Manufacturing Cost</th>
                                <th className="p-2 border">Total Cost</th>
                              </tr>
                            </thead>
                            <tbody>
                              {cost.CostBreakdownByProductType.map((row: any, idx: number) => (
                                <tr key={idx}>
                                  <td className="p-2 border">{row.ProductTypeName}</td>
                                  <td className="p-2 border">{row.RawGoldCost}</td>
                                  <td className="p-2 border">{row.ManufacturingCost}</td>
                                  <td className="p-2 border">{row.TotalCost}</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      </div>
                    )}

                    {Array.isArray(cost.CostTrend) && cost.CostTrend.length > 0 && (
                      <div>
                        <div className="font-medium mb-2">Cost Trend</div>
                        <div className="overflow-x-auto">
                          <table className="w-full text-left border text-xs">
                            <thead className="bg-muted/50">
                              <tr>
                                <th className="p-2 border">Month</th>
                                <th className="p-2 border">Total Cost</th>
                                <th className="p-2 border">Avg Cost/Gram</th>
                              </tr>
                            </thead>
                            <tbody>
                              {cost.CostTrend.map((row: any, idx: number) => (
                                <tr key={idx}>
                                  <td className="p-2 border">{row.Month}</td>
                                  <td className="p-2 border">{row.TotalCost}</td>
                                  <td className="p-2 border">{(row.AverageCostPerGram ?? 0).toFixed(2)}</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      </div>
                    )}

                    {Array.isArray(cost.TopCostProducts) && cost.TopCostProducts.length > 0 && (
                      <div>
                        <div className="font-medium mb-2">Top Cost Products</div>
                        <div className="overflow-x-auto">
                          <table className="w-full text-left border text-xs">
                            <thead className="bg-muted/50">
                              <tr>
                                <th className="p-2 border">Product</th>
                                <th className="p-2 border">Total Cost</th>
                              </tr>
                            </thead>
                            <tbody>
                              {cost.TopCostProducts.map((row: any, idx: number) => (
                                <tr key={idx}>
                                  <td className="p-2 border">{row.ProductName}</td>
                                  <td className="p-2 border">{row.TotalCost}</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      </div>
                    )}
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">No data for selected period.</p>
                )
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="workflow">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Workflow Performance</CardTitle>
                {workflow && (
                  <div className="flex gap-2">
                    {Array.isArray(workflow.WorkflowStepAnalysis) && workflow.WorkflowStepAnalysis.length > 0 && (
                      <button
                        className="h-8 px-2 border rounded text-xs"
                        onClick={() => {
                          const headers = ['StepName','TotalRecords','AverageTimeInStep','CompletionRate'];
                          exportToCsv('workflow-steps.csv', headers, workflow.WorkflowStepAnalysis);
                        }}
                      >Export Steps</button>
                    )}
                    {Array.isArray(workflow.TransitionAnalysis) && workflow.TransitionAnalysis.length > 0 && (
                      <button
                        className="h-8 px-2 border rounded text-xs"
                        onClick={() => {
                          const headers = ['FromStatus','ToStatus','TransitionCount'];
                          exportToCsv('workflow-transitions.csv', headers, workflow.TransitionAnalysis);
                        }}
                      >Export Transitions</button>
                    )}
                  </div>
                )}
              </div>
            </CardHeader>
            <CardContent>
              {loading && <p className="text-sm text-muted-foreground">Loading...</p>}
              {error && <p className="text-sm text-red-600">{error}</p>}
              {!loading && !error && (
                workflow ? (
                  <div className="space-y-4 text-sm">
                    <div className="space-y-2">
                      <div>Approval Rate: {(workflow.ApprovalRate ?? 0).toFixed(2)}%</div>
                      <div>Quality Pass Rate: {(workflow.QualityPassRate ?? 0).toFixed(2)}%</div>
                    </div>

                    {Array.isArray(workflow.WorkflowStepAnalysis) && workflow.WorkflowStepAnalysis.length > 0 && (
                      <div>
                        <div className="font-medium mb-2">Workflow Step Analysis</div>
                        <div className="overflow-x-auto">
                          <table className="w-full text-left border text-xs">
                            <thead className="bg-muted/50">
                              <tr>
                                <th className="p-2 border">Step</th>
                                <th className="p-2 border">Total Records</th>
                                <th className="p-2 border">Avg Time (hrs)</th>
                                <th className="p-2 border">Completion %</th>
                              </tr>
                            </thead>
                            <tbody>
                              {workflow.WorkflowStepAnalysis.map((row: any, idx: number) => (
                                <tr key={idx}>
                                  <td className="p-2 border">{row.StepName}</td>
                                  <td className="p-2 border">{row.TotalRecords}</td>
                                  <td className="p-2 border">{(row.AverageTimeInStep ?? 0).toFixed(2)}</td>
                                  <td className="p-2 border">{(row.CompletionRate ?? 0).toFixed(2)}%</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      </div>
                    )}

                    {Array.isArray(workflow.TransitionAnalysis) && workflow.TransitionAnalysis.length > 0 && (
                      <div>
                        <div className="font-medium mb-2">Transition Analysis</div>
                        <div className="overflow-x-auto">
                          <table className="w-full text-left border text-xs">
                            <thead className="bg-muted/50">
                              <tr>
                                <th className="p-2 border">From</th>
                                <th className="p-2 border">To</th>
                                <th className="p-2 border">Count</th>
                              </tr>
                            </thead>
                            <tbody>
                              {workflow.TransitionAnalysis.map((row: any, idx: number) => (
                                <tr key={idx}>
                                  <td className="p-2 border">{row.FromStatus}</td>
                                  <td className="p-2 border">{row.ToStatus}</td>
                                  <td className="p-2 border">{row.TransitionCount}</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      </div>
                    )}
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">No data for selected period.</p>
                )
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default ManufacturingReports;
