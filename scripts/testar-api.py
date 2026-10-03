import urllib.request, urllib.error, json
from pathlib import Path
base='http://localhost:5080/api/v1/produtos'
results=[]
def call(method,path='',body=None,expected=200):
 req=urllib.request.Request(base+path,data=json.dumps(body).encode() if body is not None else None,headers={'Content-Type':'application/json'},method=method)
 try: res=urllib.request.urlopen(req)
 except urllib.error.HTTPError as e: res=e
 text=res.read().decode(); data=json.loads(text) if text else None
 assert res.status==expected,(method,path,res.status,text)
 results.append({'method':method,'path':'/api/v1/produtos'+path,'status':res.status,'body':data})
 return data,res.headers
p={'nome':'Caderno universitário','categoria':'Papelaria','preco':24.9,'quantidade':30}
data,h=call('POST',body=p,expected=201); id=data['id']; assert h['Location'].endswith('/'+str(id))
call('GET'); call('GET','/'+str(id)); p['quantidade']=12
call('PUT','/'+str(id),p,204); data,_=call('GET','/'+str(id)); assert data['quantidade']==12
call('POST',body={'nome':'','categoria':'','preco':-1,'quantidade':-1},expected=400)
call('PUT','/'+str(id),{'nome':'','categoria':'','preco':-1,'quantidade':-1},400)
call('POST',body={'nome':'Caderno','categoria':'Papelaria'},expected=400)
call('DELETE','/'+str(id),expected=204)
for method in ['GET','DELETE']:call(method,'/'+str(id),expected=404)
call('PUT','/'+str(id),p,404)
open(Path(__file__).resolve().parents[1] / 'docs' / 'testes-http.json', 'w', encoding='utf-8').write(json.dumps(results,ensure_ascii=False,indent=2))
print(f'{len(results)} testes passaram; CRUD, validação, Location, atualização e exclusão confirmados.')
