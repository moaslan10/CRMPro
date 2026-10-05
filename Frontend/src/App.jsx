import React, { useEffect, useMemo, useState } from "react";

const API = "http://localhost:5002/api";

async function request(path, options = {}) {
  const token = localStorage.getItem("crm_token");
  const headers = {
    ...(options.body ? { "Content-Type": "application/json" } : {}),
    ...(options.headers || {})
  };
  if (token) headers.Authorization = `Bearer ${token}`;

  let response;
  try {
    response = await fetch(`${API}${path}`, { ...options, headers });
  } catch {
    throw new Error("Cannot connect to the CRM API. Start the Backend on http://localhost:5002.");
  }
  if (response.status === 401) {
    localStorage.removeItem("crm_token");
    localStorage.removeItem("crm_user");
    throw new Error("Your session expired. Please login again.");
  }
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `Request failed (${response.status})`);
  }
  if (response.status === 204) return null;
  return response.json();
}

const baseConfigs = {
  Customers: { endpoint:"customers", fields:["name","company","email","phone","status"], search:true, filter:"status" },
  Leads: { endpoint:"leads", fields:["name","company","email","phone","source","status","estimatedValue"], search:true, filter:"status" },
  Deals: { endpoint:"deals", fields:["title","customerName","value","stage","expectedCloseDate"], search:true, filter:"stage" },
  Activities: { endpoint:"activities", fields:["type","subject","relatedTo","dueDate","status"] },
  Notes: { endpoint:"notes", fields:["title","content"] }
};

function App(){
  const [user,setUser]=useState(()=>JSON.parse(localStorage.getItem("crm_user")||"null"));
  const [page,setPage]=useState("Dashboard");
  const [notifications,setNotifications]=useState({unread:0,items:[]});
  const [notifOpen,setNotifOpen]=useState(false);

  const loadNotifications=()=>{ if(!user)return; request("/notifications").then(setNotifications).catch(()=>{}); };
  useEffect(()=>{ loadNotifications(); const t=setInterval(loadNotifications,30000); return()=>clearInterval(t); },[user]);

  const logout=()=>{localStorage.removeItem("crm_token");localStorage.removeItem("crm_user");setUser(null);};
  if(!user) return <Login onLogin={setUser}/>;

  const nav = [
    ["Dashboard","📊"],["Customers","👥"],["Leads","🎯"],["Deals","💼"],["Activities","📅"],["Notes","📝"],
    ["Pipeline","🔀"],["Reports","📈"],["Users","🛡️"],["Audit Logs","🧾"],["Profile","👤"],["Settings","⚙️"]
  ];

  return <div className="app">
    <aside>
      <div className="brand">CRM<span>Pro</span></div>
      <div className="profile-box"><strong>{user.name}</strong><small>{user.role}</small></div>
      {nav.filter(([p])=>p!=="Users"&&p!=="Audit Logs" || user.role==="Admin").map(([p,icon])=>
        <button key={p} className={`nav ${page===p?"active":""}`} onClick={()=>setPage(p)}>{icon} {p}</button>
      )}
      <button className="nav logout" onClick={logout}>↪ Logout</button>
    </aside>
    <main>
      <Header page={page} user={user} notifications={notifications} open={notifOpen} setOpen={setNotifOpen} refresh={loadNotifications}/>
      {page==="Dashboard"&&<Dashboard setPage={setPage}/>}
      {baseConfigs[page]&&<CrudPage page={page} user={user}/>}
      {page==="Pipeline"&&<Pipeline/>}
      {page==="Reports"&&<Reports/>}
      {page==="Users"&&user.role==="Admin"&&<Users/>}
      {page==="Audit Logs"&&user.role==="Admin"&&<AuditLogs/>}
      {page==="Profile"&&<Profile user={user} setUser={setUser}/>}
      {page==="Settings"&&<Settings/>}
    </main>
  </div>
}

function Login({onLogin}){
  const [email,setEmail]=useState("admin@crmpro.com"),[password,setPassword]=useState("123456"),[error,setError]=useState(""),[loading,setLoading]=useState(false);
  async function submit(e){e.preventDefault();setLoading(true);setError("");try{const d=await request("/auth/login",{method:"POST",body:JSON.stringify({email,password})});localStorage.setItem("crm_token",d.token);localStorage.setItem("crm_user",JSON.stringify(d.user));onLogin(d.user)}catch(e){setError(e.message)}finally{setLoading(false)}}
  return <div className="login-page"><form className="login-card" onSubmit={submit}>
    <div className="brand">CRM<span>Pro</span></div><p className="muted">Complete CRM Workspace</p><h2>Welcome back</h2>
    <label>Email<input value={email} onChange={e=>setEmail(e.target.value)}/></label>
    <label>Password<input type="password" value={password} onChange={e=>setPassword(e.target.value)}/></label>
    {error&&<div className="error">{error}</div>}<button className="primary" disabled={loading}>{loading?"Signing in...":"Sign in"}</button>
    <small>Admin: admin@crmpro.com / 123456</small>
  </form></div>
}

function Header({page,user,notifications,open,setOpen,refresh}){
  async function readAll(){await request("/notifications/read-all",{method:"PUT"});refresh();}
  return <header><div><div className="eyebrow">CRM MANAGEMENT</div><h1>{page}</h1></div>
    <div className="header-actions"><div className="notif-wrap"><button className="icon-btn" onClick={()=>setOpen(!open)}>🔔{notifications.unread>0&&<b>{notifications.unread}</b>}</button>
      {open&&<div className="notif-panel"><div className="notif-head"><strong>Notifications</strong><button onClick={readAll}>Mark all read</button></div>
      {notifications.items.length===0?<p className="muted">No notifications</p>:notifications.items.map(n=><div className={`notification ${n.isRead?"read":""}`} key={n.id}><strong>{n.title}</strong><p>{n.message}</p><small>{new Date(n.createdAt).toLocaleString()}</small></div>)}</div>}</div>
      <div className="avatar">{user.name.charAt(0).toUpperCase()}</div></div>
  </header>
}

function Dashboard({setPage}){
  const [data,setData]=useState(null),[error,setError]=useState("");
  const load=()=>request("/dashboard").then(setData).catch(e=>setError(e.message));
  useEffect(()=>{ load(); },[]);
  if(error)return <ConnectionError message={error} onRetry={load}/>;
  if(!data)return <div className="loading">Loading dashboard...</div>;
  const cards=[["Customers",data.customers,"👥"],["Active Customers",data.activeCustomers,"✓"],["Leads",data.leads,"🎯"],["Deals",data.deals,"💼"],["Won Deals",data.wonDeals,"🏆"],["Pipeline",`${Number(data.pipelineValue).toLocaleString()} EGP`,"💰"],["Lead Value",`${Number(data.leadValue).toLocaleString()} EGP`,"📌"],["Pending Activities",data.pendingActivities,"📅"]];
  return <><div className="stats-grid">{cards.map(c=><div className="stat-card" key={c[0]}><i>{c[2]}</i><span>{c[0]}</span><strong>{c[1]}</strong><small>Live CRM data</small></div>)}</div>
    <div className="quick-grid"><div className="panel"><h2>Quick actions</h2><div className="quick-actions"><button onClick={()=>setPage("Customers")}>+ Customer</button><button onClick={()=>setPage("Leads")}>+ Lead</button><button onClick={()=>setPage("Deals")}>+ Deal</button><button onClick={()=>setPage("Pipeline")}>Open Pipeline</button></div></div>
    <div className="panel"><h2>CRM overview</h2><p className="muted">Track customers, qualify leads, manage deals and keep the entire sales process organized.</p></div></div>
  </>
}

function CrudPage({page,user}){
  const cfg=baseConfigs[page], [rows,setRows]=useState([]),[search,setSearch]=useState(""),[filter,setFilter]=useState("All"),[error,setError]=useState(""),[loading,setLoading]=useState(true),[modal,setModal]=useState(false),[editing,setEditing]=useState(null),[form,setForm]=useState({});
  const [detail,setDetail]=useState(null);
  const [pageNo,setPageNo]=useState(1); const pageSize=7;
  const load=async()=>{setLoading(true);try{let q=[];if(search)q.push(`search=${encodeURIComponent(search)}`);if(cfg.filter&&filter!=="All")q.push(`${cfg.filter}=${encodeURIComponent(filter)}`);setRows(await request(`/${cfg.endpoint}${q.length?"?"+q.join("&"):""}`));setError("")}catch(e){setError(e.message)}finally{setLoading(false)}};
  useEffect(()=>{setPageNo(1);load()},[page,search,filter]);
  const shown=rows.slice((pageNo-1)*pageSize,pageNo*pageSize), pages=Math.max(1,Math.ceil(rows.length/pageSize));
  const save=async e=>{e.preventDefault();try{const method=editing?"PUT":"POST";await request(editing?`/${cfg.endpoint}/${editing}`:`/${cfg.endpoint}`,{method,body:JSON.stringify(form)});setModal(false);setEditing(null);setForm({});load()}catch(e){setError(e.message)}};
  const remove=async id=>{if(!confirm("Delete this record?"))return;try{await request(`/${cfg.endpoint}/${id}`,{method:"DELETE"});load()}catch(e){setError(e.message)}};
  const detailCustomer=async id=>{try{setDetail(await request(`/customers/${id}/details`))}catch(e){setError(e.message)}};
  return <section className="panel"><div className="panel-header"><div><h2>{page}</h2><span className="muted">{rows.length} records</span></div><div className="toolbar">
    {cfg.search&&<input placeholder="Search..." value={search} onChange={e=>setSearch(e.target.value)}/>}
    {cfg.filter&&<select value={filter} onChange={e=>setFilter(e.target.value)}><option>All</option>{filterOptions(cfg.filter).map(x=><option key={x}>{x}</option>)}</select>}
    <button className="primary" onClick={()=>{setEditing(null);setForm(defaultForm(page));setModal(true)}}>+ Add</button></div></div>
    {error&&<div className="error">{error}</div>}
    {loading?<div className="loading small-loading">Loading...</div>:<div className="table-wrap"><table><thead><tr>{cfg.fields.map(f=><th key={f}>{label(f)}</th>)}<th>Actions</th></tr></thead><tbody>{shown.length===0?<tr><td colSpan={cfg.fields.length+1} className="empty">No records found.</td></tr>:shown.map(r=><tr key={r.id}>{cfg.fields.map(f=><td key={f}>{formatValue(r[f],f)}</td>)}<td>
      {page==="Customers"&&<button className="link" onClick={()=>detailCustomer(r.id)}>Details</button>}
      <button className="link" onClick={()=>{setEditing(r.id);setForm({...r});setModal(true)}}>Edit</button>
      <button className="danger-link" onClick={()=>remove(r.id)}>Delete</button></td></tr>)}</tbody></table></div>}
    {pages>1&&<div className="pagination"><button disabled={pageNo===1} onClick={()=>setPageNo(pageNo-1)}>Previous</button><span>Page {pageNo} of {pages}</span><button disabled={pageNo===pages} onClick={()=>setPageNo(pageNo+1)}>Next</button></div>}
    {modal&&<Modal page={page} fields={cfg.fields} form={form} setForm={setForm} editing={editing} onClose={()=>setModal(false)} onSubmit={save}/>}
    {detail&&<CustomerDetails data={detail} onClose={()=>setDetail(null)}/>}
  </section>
}

function Pipeline(){
  const [deals,setDeals]=useState([]),[error,setError]=useState(""),[drag,setDrag]=useState(null);
  const stages=["New","Proposal","Negotiation","Won","Lost"];
  const load=()=>request("/deals").then(setDeals).catch(e=>setError(e.message));
  useEffect(()=>{ load(); },[]);
  async function drop(stage){if(!drag)return;const d=deals.find(x=>x.id===drag);if(!d||d.stage===stage){setDrag(null);return}try{await request(`/deals/${d.id}`,{method:"PUT",body:JSON.stringify({...d,stage})});await load()}catch(e){setError(e.message)}setDrag(null)}
  return <section><div className="panel"><div className="panel-header"><div><h2>Deal Pipeline</h2><span className="muted">Drag a deal between stages</span></div></div>{error&&<div className="error">{error}</div>}
    <div className="pipeline">{stages.map(s=><div className="stage" key={s} onDragOver={e=>e.preventDefault()} onDrop={()=>drop(s)}><div className="stage-head"><strong>{s}</strong><span>{deals.filter(d=>d.stage===s).length}</span></div>
      {deals.filter(d=>d.stage===s).map(d=><div className="deal-card" draggable key={d.id} onDragStart={()=>setDrag(d.id)}><strong>{d.title}</strong><span>{d.customerName}</span><b>{Number(d.value).toLocaleString()} EGP</b></div>)}</div>)}</div>
  </div></section>
}

function Reports(){
  const [d,setD]=useState(null),[error,setError]=useState("");
  useEffect(()=>{ request("/reports").then(setD).catch(e=>setError(e.message)); },[]);
  if(error)return <ConnectionError message={error} onRetry={()=>location.reload()}/>;
  if(!d)return <div className="loading">Loading reports...</div>;
  const max=Math.max(1,...d.stages.map(x=>Number(x.value)));
  const exportCsv=()=>{const lines=["Stage,Deals,Value"];d.stages.forEach(x=>lines.push(`${x.stage},${x.count},${x.value}`));const blob=new Blob([lines.join("\\n")],{type:"text/csv"});const a=document.createElement("a");a.href=URL.createObjectURL(blob);a.download="crmpro-report.csv";a.click()};
  return <><div className="stats-grid four"><Metric title="Revenue" value={`${Number(d.totalRevenue).toLocaleString()} EGP`}/><Metric title="Pipeline" value={`${Number(d.totalPipeline).toLocaleString()} EGP`}/><Metric title="Win Rate" value={`${d.conversionRate}%`}/><Metric title="Customers" value={d.customers}/></div>
    <div className="panel"><div className="panel-header"><h2>Sales performance</h2><button className="primary" onClick={exportCsv}>Export CSV</button></div><div className="bars">{d.stages.map(x=><div className="bar-row" key={x.stage}><span>{x.stage}</span><div><i style={{width:`${Math.max(5,Number(x.value)/max*100)}%`}}></i></div><b>{Number(x.value).toLocaleString()} EGP</b></div>)}</div></div>
    <div className="report-grid"><div className="panel"><h2>Leads by status</h2>{d.leadStatuses.map(x=><div className="report-row" key={x.status}><span>{x.status}</span><strong>{x.count} · {Number(x.value).toLocaleString()} EGP</strong></div>)}</div><div className="panel"><h2>Customer health</h2><div className="health"><strong>{d.activeCustomers}</strong><span>Active customers</span></div><div className="health"><strong>{d.wonDeals}</strong><span>Won deals</span></div></div></div></>
}

function Users(){
  const [rows,setRows]=useState([]),[search,setSearch]=useState(""),[error,setError]=useState(""),[modal,setModal]=useState(false),[editing,setEditing]=useState(null),[form,setForm]=useState({role:"Sales",isActive:true});
  const load=()=>request(`/users${search?`?search=${encodeURIComponent(search)}`:""}`).then(setRows).catch(e=>setError(e.message));
  useEffect(()=>{ load(); },[search]);
  const save=async e=>{e.preventDefault();try{await request(editing?`/users/${editing}`:"/users",{method:editing?"PUT":"POST",body:JSON.stringify(form)});setModal(false);setEditing(null);setForm({role:"Sales",isActive:true});load()}catch(e){setError(e.message)}};
  const remove=async id=>{if(!confirm("Delete this user?"))return;try{await request(`/users/${id}`,{method:"DELETE"});load()}catch(e){setError(e.message)}};
  return <section className="panel"><div className="panel-header"><div><h2>Users</h2><span className="muted">Admin user management</span></div><div className="toolbar"><input placeholder="Search..." value={search} onChange={e=>setSearch(e.target.value)}/><button className="primary" onClick={()=>{setEditing(null);setForm({role:"Sales",isActive:true});setModal(true)}}>+ Add User</button></div></div>
    {error&&<div className="error">{error}</div>}<div className="table-wrap"><table><thead><tr><th>Name</th><th>Email</th><th>Role</th><th>Status</th><th>Created</th><th>Actions</th></tr></thead><tbody>{rows.length===0?<tr><td colSpan="6" className="empty">No users found.</td></tr>:rows.map(r=><tr key={r.id}><td>{r.name}</td><td>{r.email}</td><td>{r.role}</td><td>{r.isActive?"Active":"Disabled"}</td><td>{formatValue(r.createdAt,"createdAt")}</td><td><button className="link" onClick={()=>{setEditing(r.id);setForm({...r,password:""});setModal(true)}}>Edit</button> <button className="danger-link" onClick={()=>remove(r.id)}>Delete</button></td></tr>)}</tbody></table></div>
    {modal&&<div className="modal"><form className="modal-card" onSubmit={save}><div className="modal-header"><h2>{editing?"Edit":"Add"} User</h2><button type="button" className="close" onClick={()=>setModal(false)}>×</button></div><label>Name<input required value={form.name||""} onChange={e=>setForm({...form,name:e.target.value})}/></label><label>Email<input required type="email" value={form.email||""} onChange={e=>setForm({...form,email:e.target.value})}/></label><label>Password<input type="password" required={!editing} value={form.password||""} onChange={e=>setForm({...form,password:e.target.value})} placeholder={editing?"Leave empty to keep current password":"Password"}/></label><label>Role<select value={form.role||"Sales"} onChange={e=>setForm({...form,role:e.target.value})}><option>Sales</option><option>Admin</option></select></label><label className="switch-row"><span>Active user</span><input type="checkbox" checked={form.isActive!==false} onChange={e=>setForm({...form,isActive:e.target.checked})}/></label><button className="primary full">Save User</button></form></div>}
  </section>
}
function AuditLogs(){
  const cfg={endpoint:"audit-logs",fields:["actor","action","entity","entityId","createdAt"],search:true};return <SimpleAdminTable title="Audit Logs" config={cfg}/>
}
function SimpleAdminTable({title,config}){
  const [rows,setRows]=useState([]),[search,setSearch]=useState(""),[error,setError]=useState("");
  const load=()=>request(`/${config.endpoint}${search?`?search=${encodeURIComponent(search)}`:""}`).then(setRows).catch(e=>setError(e.message));
  useEffect(()=>{ load(); },[search]);
  const canAdd=title==="Users";
  return <section className="panel"><div className="panel-header"><h2>{title}</h2><div className="toolbar"><input placeholder="Search..." value={search} onChange={e=>setSearch(e.target.value)}/>{canAdd&&<button className="primary" onClick={()=>alert("Use the Users API for advanced provisioning. Existing users can be managed through the backend; this screen is read-first for safety.")}>+ Add User</button>}</div></div>{error&&<div className="error">{error}</div>}<div className="table-wrap"><table><thead><tr>{config.fields.map(f=><th key={f}>{label(f)}</th>)}</tr></thead><tbody>{rows.map(r=><tr key={r.id}>{config.fields.map(f=><td key={f}>{formatValue(r[f],f)}</td>)}</tr>)}</tbody></table></div></section>
}

function Profile({user,setUser}){
  const [name,setName]=useState(user.name),[password,setPassword]=useState(""),[msg,setMsg]=useState(""),[error,setError]=useState("");
  const save=async e=>{e.preventDefault();try{const d=await request("/profile",{method:"PUT",body:JSON.stringify({name,password:password||null})});localStorage.setItem("crm_user",JSON.stringify({...user,name:d.name}));setUser({...user,name:d.name});setPassword("");setMsg("Profile updated successfully.");setError("")}catch(e){setError(e.message)}};
  return <section className="panel form-panel"><h2>Profile</h2><p className="muted">Manage your personal CRM account.</p><form onSubmit={save}><label>Name<input value={name} onChange={e=>setName(e.target.value)}/></label><label>Email<input value={user.email} disabled/></label><label>Role<input value={user.role} disabled/></label><label>New password<input type="password" value={password} onChange={e=>setPassword(e.target.value)} placeholder="Leave empty to keep current password"/></label>{msg&&<div className="success">{msg}</div>}{error&&<div className="error">{error}</div>}<button className="primary">Save changes</button></form></section>
}

function Settings(){
  const [compact,setCompact]=useState(localStorage.getItem("crm_compact")==="true"),[saved,setSaved]=useState(false);
  const save=()=>{localStorage.setItem("crm_compact",compact);document.body.classList.toggle("compact-mode",compact);setSaved(true);setTimeout(()=>setSaved(false),1500)};
  return <section className="panel form-panel"><h2>Settings</h2><p className="muted">Personalize your CRM workspace.</p><label className="switch-row"><span>Compact tables</span><input type="checkbox" checked={compact} onChange={e=>setCompact(e.target.checked)}/></label>{saved&&<div className="success">Settings saved.</div>}<button className="primary" onClick={save}>Save settings</button></section>
}

function CustomerDetails({data,onClose}){
  const {customer,leads,deals,activities}=data;
  return <div className="modal"><div className="modal-card detail-card"><div className="modal-header"><div><h2>{customer.name}</h2><span className="muted">{customer.company}</span></div><button className="close" onClick={onClose}>×</button></div>
    <div className="detail-grid"><div><small>Email</small><strong>{customer.email}</strong></div><div><small>Phone</small><strong>{customer.phone}</strong></div><div><small>Status</small><strong>{customer.status}</strong></div></div>
    <h3>Related Leads ({leads.length})</h3>{leads.map(x=><div className="mini-row" key={x.id}>{x.name}<b>{x.status}</b></div>)}
    <h3>Deals ({deals.length})</h3>{deals.map(x=><div className="mini-row" key={x.id}>{x.title}<b>{Number(x.value).toLocaleString()} EGP</b></div>)}
    <h3>Activities ({activities.length})</h3>{activities.map(x=><div className="mini-row" key={x.id}>{x.subject}<b>{x.status}</b></div>)}
  </div></div>
}

function Modal({page,fields,form,setForm,editing,onClose,onSubmit}){
  return <div className="modal"><form className="modal-card" onSubmit={onSubmit}><div className="modal-header"><h2>{editing?"Edit":"Add"} {page.slice(0,-1)}</h2><button type="button" className="close" onClick={onClose}>×</button></div>{fields.map(f=><Field key={f} field={f} value={form[f]??""} onChange={v=>setForm({...form,[f]:v})}/>)}<button className="primary full">Save</button></form></div>
}
function Field({field,value,onChange}){
  const choices={status:["Active","Prospect","New","Contacted","Qualified","Pending","Completed"],source:["Website","Referral","Social Media","Advertisement"],stage:["New","Proposal","Negotiation","Won","Lost"],type:["Call","Meeting","Email","Task"]};
  if(choices[field])return <label>{label(field)}<select value={value||choices[field][0]} onChange={e=>onChange(e.target.value)}>{choices[field].map(x=><option key={x}>{x}</option>)}</select></label>;
  if(field==="content")return <label>{label(field)}<textarea value={value} rows="5" onChange={e=>onChange(e.target.value)}/></label>;
  const num=["value","estimatedValue"].includes(field),date=["dueDate","expectedCloseDate"].includes(field);
  return <label>{label(field)}<input type={num?"number":date?"date":"text"} value={date?String(value).slice(0,10):value} onChange={e=>onChange(e.target.value)}/></label>
}
function Metric({title,value}){return <div className="stat-card"><span>{title}</span><strong>{value}</strong></div>}
function ConnectionError({message,onRetry}){return <div className="connection-error"><h2>Backend connection problem</h2><p>{message}</p><button className="primary" onClick={onRetry}>Retry</button></div>}
function filterOptions(type){return type==="stage"?["New","Proposal","Negotiation","Won","Lost"]:["Active","Prospect","New","Contacted","Qualified","Pending","Completed"]}
function defaultForm(page){if(page==="Customers")return {status:"Active"};if(page==="Leads")return {source:"Website",status:"New",estimatedValue:0};if(page==="Deals")return {stage:"New",value:0,expectedCloseDate:new Date(Date.now()+30*86400000).toISOString().slice(0,10)};if(page==="Activities")return {type:"Call",status:"Pending",dueDate:new Date().toISOString().slice(0,10)};return {}}
function label(v){return v.replace(/([A-Z])/g," $1").replace(/^./,x=>x.toUpperCase())}
function formatValue(v,f){if(v===null||v===undefined)return "";if(f==="value"||f==="estimatedValue")return `${Number(v).toLocaleString()} EGP`;if(f==="createdAt"||f==="dueDate"||f==="expectedCloseDate")return String(v).slice(0,10);if(f==="isActive")return v?"Active":"Disabled";return String(v)}
export default App;
